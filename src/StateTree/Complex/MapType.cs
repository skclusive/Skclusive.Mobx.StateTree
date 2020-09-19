using Skclusive.Mobx.Observable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Skclusive.Mobx.StateTree
{
    public enum MapIdentifierMode
    {
        Unknown,

        Yes,

        No
    }

    public class MapType<S, T> : ComplexType<IMap<string, S>, IObservableMap<string, INode, T>>, IMapType, IManipulator<INode, T, string>
    {
        private MapIdentifierMode IdentifierMode { set; get; }

        private string IdentifierAttribute { set; get; }

        private ObjectNode Node { set; get; }

        private IType<S, T> SubType { set; get; }

        public MapType(string name, IType<S, T> subType) : base(name)
        {
            ShouldAttachNode = true;

            Flags = TypeFlags.Map;

            SubType = subType;

            IdentifierMode = MapIdentifierMode.Unknown;

            DeterminIdentifierMode();
        }

        public override string Describe => $"Map<string, {SubType.Describe}>";

        IType IMapType.SubType => SubType;

        private void DeterminIdentifierMode()
        {
            var objectTypes = new List<IObjectType>();

            TryCollectObjectType(SubType, objectTypes);

            if (objectTypes.Count > 0)
            {
                string identifierAttribute = "";

                foreach (var objectType in objectTypes)
                {
                    var objIdentifierAttribute = objectType.IdentifierAttribute;
                    if (!string.IsNullOrWhiteSpace(objIdentifierAttribute))
                    {
                        if (!string.IsNullOrWhiteSpace(identifierAttribute) && identifierAttribute != objIdentifierAttribute)
                        {
                            throw new Exception($"The objects in a map should all have the same identifier attribute, expected '{identifierAttribute}', but child of type '{objectType.Name}' declared attribute '${objIdentifierAttribute}' as identifier");
                        }

                        identifierAttribute = objIdentifierAttribute;
                    }
                }

                if (!string.IsNullOrWhiteSpace(identifierAttribute))
                {
                    IdentifierAttribute = identifierAttribute;

                    IdentifierMode = MapIdentifierMode.Yes;
                }
                else
                {
                    IdentifierMode = MapIdentifierMode.No;
                }
            }
        }

        public override IMap<string, INode> InitializeChildNodes(INode node, object snapshot)
        {
            IDictionary<string, object> values = new Dictionary<string, object>();

            if (snapshot is IMap<string, S> dictionary)
            {
                foreach(var item in dictionary.Keys)
                {
                    values[item.ToString()] = dictionary[item];
                }
            }

            IEnvironment env = node.Environment;

            return values.Aggregate(new Map<string, INode>(), (map, pair) =>
            {
                var subpath = $"{pair.Key}";

                map[subpath] = SubType.Instantiate(node, subpath, env, pair.Value);

                return map;
            });
        }

        public override INode Instantiate(INode parent, string subpath, IEnvironment environment, object initialValue)
        {
            if (IdentifierMode == MapIdentifierMode.Unknown)
            {
                DeterminIdentifierMode();
            }
            return this.CreateNode(parent as ObjectNode, subpath, environment, initialValue, (childNodes) => CreateNewInstance(childNodes as IMap<string, INode>), (node, snapshot) => FinalizeNewInstance(node as ObjectNode));
        }

        private IObservableMap<string, INode, T> CreateNewInstance(IMap<string, INode> childNodes)
        {
            return ObservableMap<string, INode, T>.FromIn(childNodes, null, this);
            // addHiddenFinalProp(map, "put", put)
        }

        private void FinalizeNewInstance(ObjectNode node)
        {
            var objNode = node;

            var instance = objNode.StoredValue as IObservableMap<string, INode, T>;

            Node = objNode;

            instance.Intercept(change => WillChange(change));

            instance.Observe(change => DidChange(change));
        }

        public override IObservableMap<string, INode, T> GetValue(INode node)
        {
            return node.StoredValue as IObservableMap<string, INode, T>;
        }

        public override IReadOnlyCollection<INode> GetChildren(INode node)
        {
            return GetValue(node).GetValues().ToList();
        }

        public override INode GetChildNode(INode node, string key)
        {
            return GetValue(node).GetValue(key);
        }

        public override IType GetChildType(string key)
        {
            return SubType;
        }

        public override IMap<string, S> GetSnapshot(INode node, bool applyPostProcess)
        {
            IMap<string, S> snapshot = new Map<string, S>();

            foreach (var cnode in GetChildren(node))
            {
                snapshot[cnode.Subpath] = (S)cnode.Snapshot;
            }

            return snapshot;
        }

        public override void RemoveChild(INode node, string subpath)
        {
            GetValue(node).Remove(subpath);
        }

        protected override IMap<string, S> GetDefaultSnapshot()
        {
            return new Map<string, S>();
        }

        protected override IValidationError[] IsValidSnapshot(object values, IContextEntry[] context)
        {
            if (values is IDictionary<string, S> dictionary)
            {
                var errors = dictionary.Keys.Select(key => SubType.Validate(dictionary[key], StateTreeUtils.GetContextForPath(context, $"{key}", SubType)));

                return errors.Aggregate(new IValidationError[] { }, (acc, value) => acc.Concat(value).ToArray());
            }

            return new IValidationError[]
            {
                new ValidationError
                {
                    Context = context,

                    Value = values,

                    Message = $"Value is not an dictionary"
                }
            };
        }

        public override void ApplyPatchLocally(INode node, string subpath, IJsonPatch patch)
        {
            var map = GetValue(node);

            switch (patch.Operation)
            {
                case JsonPatchOperation.Add:
                case JsonPatchOperation.Replace:
                    map[subpath] = SubType.Create((S)patch.Value, Node.Environment);
                    break;

                case JsonPatchOperation.Remove:
                    map.Remove(subpath);
                    break;
            }
        }

        public override void ApplySnapshot(INode node, IMap<string, S> snapshot)
        {
            StateTreeUtils.Typecheck(this, snapshot);

            var map = GetValue(node);

            var keysmap = map.Keys.Aggregate(new Map<string, bool>(), (acc, key) =>
            {
                acc[key] = false;
                return acc;
            });

            foreach (var pair in snapshot)
            {
                map[pair.Key] = SubType.Create(pair.Value, Node.Environment);
                keysmap[pair.Key] = true;
            }

            foreach (var pair in keysmap)
            {
                if (!pair.Value)
                {
                    map.Remove(pair.Key);
                }
            }
        }

        private void ProcessIdentifier(string expected, INode node)
        {
            if (node is ObjectNode objectNode)
            {
                if (IdentifierMode == MapIdentifierMode.Yes)
                {
                    string identifier = objectNode.Identifier;
                    if (identifier != expected)
                    {
                        throw new InvalidOperationException($"A map of objects containing an identifier should always store the object under their own identifier. Trying to store key '{identifier}', but expected: '{expected}'");
                    }
                }
            }
        }

        private IMapWillChange<string, INode> WillChange(IMapWillChange<string, INode> change)
        {
            var node = change.Object.GetStateTreeNode() as ObjectNode;

            node.AssertWritable();

            var map = change.Object as IObservableMap<string, INode, T>;

            switch (change.Type)
            {
                case ChangeType.UPDATE:
                    {
                        var oldValue = map.GetValue(change.Name);

                        if (change.NewValue == oldValue)
                        {
                            return null;
                        }

                        StateTreeUtils.Typecheck(SubType, change.NewValue.StoredValue);

                        change.NewValue = SubType.Reconcile(node.GetChildNode(change.Name), change.NewValue.StoredValue);

                        ProcessIdentifier(change.Name, change.NewValue);
                    }
                    break;
                case ChangeType.ADD:
                    {
                        StateTreeUtils.Typecheck(SubType, change.NewValue.StoredValue);

                        change.NewValue = SubType.Instantiate(node, change.Name, Node.Environment, change.NewValue.StoredValue);

                        ProcessIdentifier(change.Name, change.NewValue);
                    }
                    break;
            }

            return change;
        }

        private void DidChange(IMapDidChange<string, INode> change)
        {
            var node = change.Object.GetStateTreeNode() as ObjectNode;

            switch (change.Type)
            {
                case ChangeType.UPDATE:

                    node.Emitpatch(new ReversibleJsonPatch
                    {
                        Operation = JsonPatchOperation.Replace,

                        Path = change.Name.EscapeJsonPath(),

                        Value = change.NewValue.Snapshot,

                        OldValue = change.OldValue?.Snapshot

                    }, node);

                    break;
                case ChangeType.ADD:

                    node.Emitpatch(new ReversibleJsonPatch
                    {
                        Operation = JsonPatchOperation.Add,

                        Path = change.Name.EscapeJsonPath(),

                        Value = change.NewValue.Snapshot

                    }, node);

                    break;

                case ChangeType.REMOVE:

                    // a node got deleted, get the old snapshot and make the node die
                    var oldSnapshot = change.OldValue.Snapshot;
                    change.OldValue.Dispose();
                    // emit the patch

                    node.Emitpatch(new ReversibleJsonPatch
                    {
                        Operation = JsonPatchOperation.Remove,

                        Path = change.Name.EscapeJsonPath(),

                        OldValue = oldSnapshot

                    }, node);

                    break;
            }
        }


        public INode Enhance(INode newv, INode oldV, object name)
        {
            return newv;
        }

        public T Dehance(INode node)
        {
            return (T)(Node?.Unbox(node) ?? node?.Value);
        }

        public object Enhance(object newv, object oldV, object name)
        {
            return newv;
        }

        public object Dehance(object value)
        {
            return Dehance(value as INode);
        }

        public INode Enhance(T value)
        {
            return (INode)Enhance((object)value);
        }

        public object Enhance(object value)
        {
            // TempNode to help out WillChange behaviour

            return new TempNode
            {
                Type = SubType,

                Value = value,

                StoredValue = value,

                Snapshot = value,
            };
        }

        public INode Enhance(INode newv, INode oldV, string name)
        {
            return newv;
        }

        private static void TryCollectObjectType(IType type, List<IObjectType> objectTypes)
        {
            if (type is IObjectType objectType)
            {
                objectTypes.Add(objectType);
            }
            else if (type is IOptionalType optionalType)
            {
                TryCollectObjectType(optionalType.SubType, objectTypes);
            }
            else if (type is ILateType lateType)
            {
                try
                {
                    TryCollectObjectType(lateType.SubType, objectTypes);
                } catch(Exception)
                {
                }
            }
            else if (type is IUnionType unionType)
            {
                foreach (var subtype in unionType.SubTypes)
                {
                    TryCollectObjectType(subtype, objectTypes);
                }
            }
        }
    }
}
