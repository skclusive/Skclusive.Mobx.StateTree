using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Skclusive.Core.Collection;
using Skclusive.Mobx.Observable;

namespace Skclusive.Mobx.StateTree
{
    public class LateObjectType<S, T> : IObjectType<S, T>, ILateType
    {
        public string Name { set; get; }

        public LateObjectType(string name, Func<IObjectType<S, T>> definition)
        {
            Name = name;

            Definition = definition;
        }

        private Func<IObjectType<S, T>> Definition { set; get; }

        private IObjectType<S, T> _SubType;

        private IObjectType<S, T> SubType
        {
            get
            {
                if (_SubType == null)
                {
                    _SubType = Definition();

                    if (_SubType == null)
                    {
                        throw new Exception("Failed to determine subtype, make sure types.late returns a type definition.");
                    }
                }

                return _SubType;
            }
        }

        public IReadOnlyCollection<Func<object, object>> Initializers => SubType.Initializers;

        public IReadOnlyDictionary<string, IType> Properties => SubType.Properties;

        public IReadOnlyCollection<IMutableProperty> Mutables => SubType.Mutables;

        public IReadOnlyCollection<IViewProperty> Views => SubType.Views;

        public IReadOnlyCollection<IActionProperty> Actions => SubType.Actions;

        public T Type => SubType.Type;

        public S SnapshotType => SubType.SnapshotType;

        public TypeFlags Flags => SubType.Flags;

        public bool IsType => SubType.IsType;

        public string Describe => SubType.Describe;

        public bool ShouldAttachNode => SubType.ShouldAttachNode;

        object IType.Type => ((IType)SubType).Type;

        object IType.SnapshotType => ((IType)SubType).SnapshotType;

        public string IdentifierAttribute => SubType.IdentifierAttribute;

        IType ILateType.SubType => SubType;

        public IObjectType<S, T> Include<Sx, Tx>(IObjectType<Sx, Tx> type)
        {
            return SubType.Include(type);
        }

        public IObjectType<S, T> Named(string name)
        {
            return SubType.Named(name);
        }

        public IObjectType<S, T> Proxy(Func<IObservableObject<T, INode>, T> proxify)
        {
            return SubType.Proxy(proxify);
        }

        public IObjectType<S, T> Snapshot(Func<S> snpashoty)
        {
            return SubType.Snapshot(snpashoty);
        }

        public IObjectType<S, T> PreProcessSnapshot(Func<object, S> fn)
        {
            return SubType.PreProcessSnapshot(fn);
        }

        public IObjectType<S, T> Mutable<P>(Expression<Func<T, P>> expression, IType type, P defaultValue = default)
        {
            return SubType.Mutable(expression, type, defaultValue);
        }

        public IObjectType<S, T> View<P>(Expression<Func<T, P>> expression, IType type, Func<T, P> view)
        {
            return SubType.View(expression, type, view);
        }

        public IObjectType<S, T> Action<R>(Expression<Action<T>> expression, Func<T, R> func)
        {
            return SubType.Action(expression, func);
        }

        public IObjectType<S, T> Action<A1, R>(Expression<Action<T>> expression, Func<T, A1, R> func)
        {
            return SubType.Action(expression, func);
        }

        public IObjectType<S, T> Action<A1, A2, R>(Expression<Action<T>> expression, Func<T, A1, A2, R> func)
        {
            return SubType.Action(expression, func);
        }

        public IObjectType<S, T> Action<A1, A2, A3, R>(Expression<Action<T>> expression, Func<T, A1, A2, A3, R> func)
        {
            return SubType.Action(expression, func);
        }

        public IObjectType<S, T> Action<A1, A2, A3, A4, R>(Expression<Action<T>> expression, Func<T, A1, A2, A3, A4, R> func)
        {
            return SubType.Action(expression, func);
        }

        public IObjectType<S, T> Action(Expression<Action<T>> expression, Action<T> func)
        {
            return SubType.Action(expression, func);
        }

        public IObjectType<S, T> Action<A1>(Expression<Action<T>> expression, Action<T, A1> func)
        {
            return SubType.Action(expression, func);
        }

        public IObjectType<S, T> Action<A1, A2>(Expression<Action<T>> expression, Action<T, A1, A2> func)
        {
            return SubType.Action(expression, func);
        }

        public IObjectType<S, T> Action<A1, A2, A3>(Expression<Action<T>> expression, Action<T, A1, A2, A3> func)
        {
            return SubType.Action(expression, func);
        }

        public IObjectType<S, T> Action<A1, A2, A3, A4>(Expression<Action<T>> expression, Action<T, A1, A2, A3, A4> func)
        {
            return SubType.Action(expression, func);
        }

        public T GetValue(INode node)
        {
            return SubType.GetValue(node);
        }

        public S GetSnapshot(INode node, bool applyPostProcess)
        {
            return SubType.GetSnapshot(node, applyPostProcess);
        }

        public T Create(S snapshot = default, IEnvironment environment = null)
        {
            return SubType.Create(snapshot, environment);
        }

        public void ApplySnapshot(INode node, S snapshot)
        {
            SubType.ApplySnapshot(node, snapshot);
        }

        public bool Is(object thing)
        {
            return SubType.Is(thing);
        }

        public IValidationError[] Validate(object thing, IContextEntry[] context)
        {
            return SubType.Validate(thing, context);
        }

        public INode Instantiate(INode parent, string subpath, IEnvironment environment, object initialValue)
        {
            return SubType.Instantiate(parent, subpath, environment, initialValue);
        }

        public INode Reconcile(INode current, object newValue)
        {
            return SubType.Reconcile(current, newValue);
        }

        public object Create(object snapshot, IEnvironment environment)
        {
            return SubType.Create(snapshot, environment);
        }

        object IType.GetValue(INode node)
        {
            return ((IType)SubType).GetValue(node);
        }

        object IType.GetSnapshot(INode node, bool applyPostProcess)
        {
            return ((IType)SubType).GetSnapshot(node, applyPostProcess);
        }

        public void ApplySnapshot(INode node, object snapshot)
        {
            SubType.ApplySnapshot(node, snapshot);
        }

        public void ApplyPatchLocally(INode node, string subpath, IJsonPatch patch)
        {
            SubType.ApplyPatchLocally(node, subpath, patch);
        }

        public IReadOnlyCollection<INode> GetChildren(INode node)
        {
            return SubType.GetChildren(node);
        }

        public INode GetChildNode(INode node, string key)
        {
            return SubType.GetChildNode(node, key);
        }

        public IType GetChildType(string key)
        {
            return SubType.GetChildType(key);
        }

        public void RemoveChild(INode node, string subpath)
        {
            SubType.RemoveChild(node, subpath);
        }

        public bool IsAssignableFrom(IType type)
        {
            return SubType.IsAssignableFrom(type);
        }

        public IMap<string, INode> InitializeChildNodes(INode node, object snapshot)
        {
            return SubType.InitializeChildNodes(node, snapshot);
        }
    }
}
