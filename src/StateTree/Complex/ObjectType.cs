using Skclusive.Core.Collection;
using Skclusive.Mobx.Observable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Skclusive.Mobx.StateTree
{
    public abstract class ObjectType<S, T, I> : ComplexType<S, T>, IObjectType<S, T, I>, IManipulator<INode, object>
        where I : IObjectType<S, T, I>
    {
        protected Func<object, object> PreProcessor { set; get; }

        protected Func<IObservableObject<T, INode>, T> Proxify { set; get; }

        protected Func<S> Snapshoty { set; get; }

        // protected ObjectNode Node { set; get; }

        public string IdentifierAttribute { get; private set; }

        protected ObjectType(string name = "AnonymousObject") : base(name)
        {
        }

        protected ObjectType(IObjectTypeConfig<S, T> config) : base(config.Name ?? "AnonymousObject")
        {
            ShouldAttachNode = true;

            Flags = TypeFlags.Object;

            Initializers = config.Initializers;

            PreProcessor = config.PreProcessor;

            Mutables = config.Mutables;

            Views = config.Views;

            Actions = config.Actions;

            Proxify = config.Proxify;

            Snapshoty = config.Snapshoty;

            Properties = ToProperties();

            IdentifierAttribute = FindIdentifierAttribute();
        }

        private string FindIdentifierAttribute()
        {
            var identifiers = Properties
                // not to evaluate late types. Late types may not be Identifier type
                .Where(property => !(property.Value is ILateType))
                .Where(property => property.Value.Flags == TypeFlags.Identifier).Select(property => property.Key);

            var identifierAttribute = "";

            foreach (var identifier in identifiers)
            {
                if (!string.IsNullOrWhiteSpace(identifierAttribute))
                {
                    throw new Exception($"Cannot define property '{identifier}' as object identifier, property '{identifierAttribute}' is already defined as identifier property");
                }

                identifierAttribute = identifier;
            }

            return identifierAttribute;
        }

        public IReadOnlyCollection<Func<object, object>> Initializers { private set; get; } = new List<Func<object, object>>();

        public IReadOnlyDictionary<string, IType> Properties { private set; get; } = new Dictionary<string, IType>();

        public IReadOnlyCollection<IMutableProperty> Mutables { private set; get; } = new List<IMutableProperty>();

        public IReadOnlyCollection<IViewProperty> Views { private set; get; } = new List<IViewProperty>();

        public IReadOnlyCollection<IActionProperty> Actions { private set; get; } = new List<IActionProperty>();

        public IEnumerable<string> PropertyNames => Properties.Keys;

        public override string Describe
        {
            get
            {
                var props = PropertyNames.Select(key => $"{key}: {Properties[key].Describe}").Join("; ");

                return $"{{ {props} }}";
            }
        }

        public override void ApplyPatchLocally(INode node, string subpath, IJsonPatch patch)
        {
            if (!(patch.Operation == JsonPatchOperation.Add || patch.Operation == JsonPatchOperation.Replace))
            {
                throw new InvalidOperationException($"object does not support operation ${patch.Operation}");
            }

            SetValue(node, subpath, patch.Value);
        }

        protected void SetValue(INode node, string property, object value)
        {
            if (node is ObjectNode objectNode)
            {
                SetValue(objectNode, property, value);
            }
        }

        protected void SetValue(ObjectNode node, string property, object value)
        {
            if (node.StoredValue is IObservableObject<T, INode> observable)
            {
                observable.Write(property, value);
            }
        }

        public override void ApplySnapshot(INode node, S snapshot)
        {
            var value = (S)ApplySnapshotPreProcessor(snapshot);

            StateTreeUtils.Typecheck(this, value);

            if (node.StoredValue is IObservableObject<T, INode> observable)
            {
                foreach (var property in PropertyNames)
                {
                    observable.Write(property, GetPropertyValue(value, property));
                }
            }
        }

        protected abstract object GetPropertyValue(S snapshot, string property);

        protected abstract void SetPropertyValue(S snapshot, string property, object value);

        public override INode GetChildNode(INode node, string key)
        {
            if (!Properties.ContainsKey(key))
            {
                throw new InvalidOperationException($"Not a value property: {key}");
            }

            var observable = node.StoredValue as IObservableObject<T, INode>;

            INode child = (INode)observable.Get(key);

            if (child == null)
            {
                throw new InvalidOperationException($"Node not available for property {key}");
            }

            return child;
        }

        public override IReadOnlyCollection<INode> GetChildren(INode node)
        {
            return PropertyNames.Select(property => GetChildNode(node, property)).ToList();
        }

        public override IType GetChildType(string key)
        {
            return Properties[key];
        }

        // TODO : incomplete
        public override S GetSnapshot(INode node, bool applyPostProcess)
        {
            S snapshot = GetDefaultSnapshot();

            foreach (var property in PropertyNames)
            {
                var depTreeNode = TypeUtils.GetAtom(node.StoredValue, property, true);

                if (depTreeNode is IAtom atom)
                {
                    atom.ReportObserved();
                }

                SetPropertyValue(snapshot, property, GetChildNode(node, property).Snapshot);
            }

            if (applyPostProcess && node.StoredValue is IDictionary<string, object> svalue)
            {
                Func<object, object> action = svalue[Hooks.PostProcessSnapshot.ToString()] as Func<object, object>;

                if (action != null)
                {
                    return (S)action.Invoke(snapshot);
                }
            }

            return snapshot;
        }

        public override T GetValue(INode node)
        {
            return (T)node.StoredValue;
        }

        public override void RemoveChild(INode node, string subpath)
        {
            if (node is ObjectNode onode)
            {
                if (onode.StoredValue is IObservableObject<T, INode> observable)
                {
                    observable.Remove(subpath);
                }
            }
        }

        protected override S GetDefaultSnapshot()
        {
            S snapshot = Snapshoty();

            return snapshot;
        }

        protected object ApplySnapshotPreProcessor(object snapshot)
        {
            return PreProcessor?.Invoke(snapshot) ?? snapshot;
        }

        protected override IValidationError[] IsValidSnapshot(object snapshot, IContextEntry[] context)
        {
            var value = ApplySnapshotPreProcessor(snapshot);

            if (value == null || value != null && !(value is S))
            {
                return new IValidationError[]
                {
                    new ValidationError
                    {
                        Context = context,

                        Value = snapshot,

                        Message = $"Value is not a valid Snapshot object type. Current: {value.GetType().FullName} Expected: {typeof(S).FullName}"
                    }
                };
            }

            return PropertyNames.Select(property =>
            {
                var type = GetChildType(property);

                var contexts = context.Concat
                (
                    new IContextEntry[]
                    {
                        new ContextEntry { Path = property, Type = type }
                    }
                ).ToArray();

                return type.Validate(GetPropertyValue((S)snapshot, property) ?? GetMutableDefault(property), contexts);
            })
            .Aggregate((errors, errs) => errors.Concat(errs).ToArray());
        }

        private object GetMutableDefault(string property)
        {
            return Mutables.Where(mutable => mutable.Name == property).FirstOrDefault()?.Default;
        }

        public override INode Instantiate(INode parent, string subpath, IEnvironment environment, object initialValue)
        {
            // Optimization: record all prop- view- and action names after first construction, and generate an optimal base class
            // that pre-reserves all these fields for fast object-member lookups
            var snapshot = initialValue.IsStateTreeNode() ? initialValue : ApplySnapshotPreProcessor(initialValue);
            return this.CreateNode(parent as ObjectNode, subpath, environment, snapshot, (childNodes, meta) => CreateNewInstance(childNodes as IMap<string, INode>, meta), (node, childNodes, meta) => FinalizeNewInstance(node, childNodes as IMap<string, INode>));
        }

        public override IMap<string, INode> InitializeChildNodes(INode node, object snapshot)
        {
            IEnvironment env = node.Environment;

            return Properties.Aggregate(new Map<string, INode>(), (map, pair) =>
            {
                var subpath = $"{pair.Key}";

                map[subpath] = pair.Value.Instantiate(node, subpath, env, GetPropertyValue((S)snapshot, pair.Key) ?? GetMutableDefault(pair.Key));

                return map;
            });
        }

        protected T CreateNewInstance(IMap<string, INode> childNodes, IStateTreeNode meta)
        {
            var observables = Mutables.Select(mutable => new ObservableProperty { Type = mutable.Kind, Name = mutable.Name, Default = mutable.Default }).ToList();

            var computeds = Views.Select(view => new ComputedProperty { Type = view.Kind, Name = view.Name, Compute = view.View }).ToList();

            // var actions = Actions.Select(action => new ActionMethod { Name = action.Name, Action = action.Action });

            ObservableTypeDef typeDef = new ObservableTypeDef(observables, computeds);

            var instance = ObservableObject<T, INode>.FromAs(typeDef, Proxify, Name, this, meta);

            return instance;
        }

        protected void FinalizeNewInstance(INode node, IMap<string, INode> childNodes)
        {
            var instance = node.StoredValue as IObservableObject<T, INode>;

            // Node = node as ObjectNode;

            if (instance != null)
            {
                foreach (var property in childNodes)
                {
                    instance.Write(property.Key, property.Value);
                }

                foreach (var initializer in Initializers)
                {
                    initializer(instance);
                }

                if (instance is IObservableObject<T, INode> observable)
                {
                    foreach (var action in Actions)
                    {
                        observable.AddAction(action.Name, StateTreeAction.CreateActionInvoker(instance, action.Name, action.Action));
                    }
                }

                instance.Intercept(change => WillChange(change));

                instance.Observe(change => DidChange(change));
            }
        }

        private IObjectWillChange WillChange(IObjectWillChange change)
        {
            var node = change.Object.GetStateTreeNode();

            node.AssertWritable();

            // only properties are typed, state are stored as-is references
            var type = Properties.ContainsKey(change.Name) ? Properties[change.Name] : null;

            if (type != null)
            {
                StateTreeUtils.Typecheck(type, change.NewValue);

                change.NewValue = type.Reconcile(node.GetChildNode(change.Name), change.NewValue);
            }

            return change;
        }

        private void DidChange(IObjectDidChange change)
        {
            if (!Properties.ContainsKey(change.Name))
            {
                return;
            }

            var node = change.Object.GetStateTreeNode();
            var newValue = change.NewValue as INode;
            var oldValue = change.OldValue as INode;

            node.Emitpatch(new ReversibleJsonPatch
            {
                Operation = JsonPatchOperation.Replace,

                Path = $"{change.Name}",

                Value = newValue.Snapshot,

                OldValue = oldValue?.Snapshot

            }, node);
        }

        private IReadOnlyDictionary<string, IType> ToProperties()
        {
            return Mutables.ToDictionary(x => x.Name, y => y.Type);
        }

        public I Named(string name)
        {
            return EnhanceWith(new ObjectTypeConfig<S, T> { Name = name });
        }

        public I Proxy(Func<IObservableObject<T, INode>, T> proxify)
        {
            return EnhanceWith(new ObjectTypeConfig<S, T> { Proxify = proxify });
        }

        public I Include<Sx, Tx>(IObjectType<Sx, Tx> type)
        {
            return EnhanceWith(new ObjectTypeConfig<S, T> { Name = type.Name, Initializers = type.Initializers, Mutables = type.Mutables, Actions = type.Actions, Properties = type.Properties, Views = type.Views });
        }

        public I Snapshot(Func<S> snpashoty)
        {
            return EnhanceWith(new ObjectTypeConfig<S, T> { Snapshoty = snpashoty });
        }

        public I PreProcessSnapshot(Func<object, S> preProcess)
        {
            var current = PreProcessor;

            Func<object, object> preProcessor = (snapshot) =>
            {
                var processed = preProcess(snapshot);

                return current == null ? processed : current(processed);
            };

            return EnhanceWith(new ObjectTypeConfig<S, T> { PreProcessor = preProcessor });
        }

        public I Mutable<P>(Expression<Func<T, P>> expression, IType type, P defaultValue = default(P))
        {
            var property = ExpressionUtils.GetPropertySymbol(expression);

            return EnhanceWith(new ObjectTypeConfig<S, T>
            {
                Mutables = new IMutableProperty[]
                {
                    new MutableProperty
                    {
                        Name = property,

                        Default = defaultValue,

                        Kind = typeof(P),

                        Type = type
                    }
                }
            });
        }

        public I View<P>(Expression<Func<T, P>> expression, IType type, Func<T, P> view)
        {
            var property = ExpressionUtils.GetPropertySymbol(expression);

            return EnhanceWith(new ObjectTypeConfig<S, T>
            {
                Views = new IViewProperty[]
                {
                    new ViewProperty
                    {
                        Name = property,

                        View = (obj) => view((T)obj),

                        Kind = typeof(P),

                        Type = type
                    }
                }
            });
        }

        #region Actions

        private I Action(Expression<Action<T>> expression, Func<object[], object> action)
        {
            var name = ExpressionUtils.GetMethodSymbol(expression);

            return EnhanceWith(new ObjectTypeConfig<S, T>
            {
                Actions = new IActionProperty[]
                {
                    new ActionProperty
                    {
                        Name = name,

                        Action = action
                    }
                }
            });
        }

        private I Action(Expression<Action<T>> expression, Action<object[]> action)
        {
            return Action(expression, (arguments) =>
            {
                action(arguments);
                return null;
            });
        }

        public I Action<R>(Expression<Action<T>> expression, Func<T, R> func)
        {
            return Action(expression, func.Pack());
        }

        public I Action<A1, R>(Expression<Action<T>> expression, Func<T, A1, R> func)
        {
            return Action(expression, func.Pack());
        }

        public I Action<A1, A2, R>(Expression<Action<T>> expression, Func<T, A1, A2, R> func)
        {
            return Action(expression, func.Pack());
        }

        public I Action<A1, A2, A3, R>(Expression<Action<T>> expression, Func<T, A1, A2, A3, R> func)
        {
            return Action(expression, func.Pack());
        }

        public I Action<A1, A2, A3, A4, R>(Expression<Action<T>> expression, Func<T, A1, A2, A3, A4, R> func)
        {
            return Action(expression, func.Pack());
        }

        public I Action(Expression<Action<T>> expression, Action<T> func)
        {
            return Action(expression, func.Pack());
        }

        public I Action<A1>(Expression<Action<T>> expression, Action<T, A1> func)
        {
            return Action(expression, func.Pack());
        }

        public I Action<A1, A2>(Expression<Action<T>> expression, Action<T, A1, A2> func)
        {
            return Action(expression, func.Pack());
        }

        public I Action<A1, A2, A3>(Expression<Action<T>> expression, Action<T, A1, A2, A3> func)
        {
            return Action(expression, func.Pack());
        }

        public I Action<A1, A2, A3, A4>(Expression<Action<T>> expression, Action<T, A1, A2, A3, A4> func)
        {
            return Action(expression, func.Pack());
        }

        #endregion

        protected IObjectTypeConfig<S, T> Config(IObjectTypeConfig<S, T> config)
        {
            return new ObjectTypeConfig<S, T>
            {
                Name = config.Name ?? Name,

                Mutables = config.Mutables.Concat(Mutables).ToList(),

                Views = Views.Concat(config.Views).Distinct().ToList(),

                Actions = Actions.Concat(config.Actions).Distinct().ToList(),

                Properties = Properties.Concat(config.Properties).Distinct().ToDictionary(x => x.Key, y => y.Value),

                Initializers = config.Initializers.Concat(Initializers).ToList(),

                PreProcessor = config.PreProcessor ?? PreProcessor,

                Proxify = config.Proxify ?? Proxify,

                Snapshoty = config.Snapshoty ?? Snapshoty
            };
        }

        private I EnhanceWith(IObjectTypeConfig<S, T> config)
        {
            return Enhance(Config(config));
        }

        protected abstract I Enhance(IObjectTypeConfig<S, T> config);

        #region IManipulator

        public INode Enhance(object value)
        {
            return (INode)value;
        }

        public INode Enhance(INode newv, INode oldV, object name)
        {
            return newv;
        }

        object IManipulator.Enhance(object value)
        {
            return value;
        }

        public object Enhance(object newv, object oldV, object name)
        {
            return newv;
        }

        public object Dehance(object value)
        {
            return Dehance(value as INode);
        }

        public object Dehance(INode node)
        {
            //if (node.Parent is ObjectNode parentNode)
            //{
            //    return (T)parentNode.Unbox(node);
            //}
            //else if (node is ObjectNode objectNode)
            //{
            //    return (T)objectNode.Unbox(node);
            //}

            if (node != null && node.Parent != null)
            {
                node.Parent.AssertAlive();
            }

            if (node != null && node.AutoUnbox)
            {
                return node.Value;
            }
            return node;
        }

        #endregion
    }

    public class ObjectType<S, T> : ObjectType<S, T, IObjectType<S, T>>, IObjectType<S, T>
    {
        public ObjectType(string name = "AnonymousObject") : base(name)
        {
        }

        public ObjectType(IObjectTypeConfig<S, T> config) : base(config)
        {
        }

        protected override IObjectType<S, T> Enhance(IObjectTypeConfig<S, T> config)
        {
            return new ObjectType<S, T>(config);
        }

        protected override S GetDefaultSnapshot()
        {
            return Snapshoty != null ? base.GetDefaultSnapshot() : Activator.CreateInstance<S>();
        }

        protected override object GetPropertyValue(S snapshot, string property)
        {
            return StateTreeUtils.GetPropertyValue(snapshot, property);
        }

        protected override void SetPropertyValue(S snapshot, string property, object value)
        {
            StateTreeUtils.SetPropertyValue(snapshot, property, value);
        }
    }

    public class ObjectType<T> : ObjectType<IDictionary<string, object>, T, IObjectType<T>>, IObjectType<T>
    {
        public ObjectType(string name = "AnonymousObject") : base(name)
        {
        }

        public ObjectType(IObjectTypeConfig<IDictionary<string, object>, T> config) : base(config)
        {
        }

        protected override IObjectType<T> Enhance(IObjectTypeConfig<IDictionary<string, object>, T> config)
        {
            return new ObjectType<T>(config);
        }

        protected override IDictionary<string, object> GetDefaultSnapshot()
        {
            return Snapshoty != null ? base.GetDefaultSnapshot() : new Dictionary<string, object>();
        }

        protected override object GetPropertyValue(IDictionary<string, object> snapshot, string property)
        {
            if (snapshot == null)
            {
                return null;
            }
            return snapshot.ContainsKey(property) ? snapshot[property] : null;
        }

        protected override void SetPropertyValue(IDictionary<string, object> snapshot, string property, object value)
        {
            if (snapshot != null)
            {
                snapshot[property] = value;
            }
        }
    }
}
