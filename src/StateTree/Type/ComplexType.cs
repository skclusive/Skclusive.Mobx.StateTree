using System;
using System.Collections.Generic;
using System.Linq;
using Skclusive.Mobx.Observable;

namespace Skclusive.Mobx.StateTree
{
    public abstract class ComplexType<S, T> : IType<S, T> //, IComplexType<S, T> where T : IStateTreeNode, ISnapshottable<S>
    {
        public string Name { get; set; }

        public bool IsType { get => true; }

        public ComplexType(string name)
        {
            Name = name;
        }

        public T Create(S snapshot = default(S), IEnvironment environment = null)
        {
            snapshot = snapshot != null ? snapshot : GetDefaultSnapshot();

            StateTreeUtils.Typecheck(this, snapshot);

            return (T)Instantiate(null, "", environment, snapshot).Value;
        }

        public virtual TypeFlags Flags { protected set; get; }

        public virtual bool ShouldAttachNode { get; protected set; }

        public abstract INode Instantiate(INode parent, string subpath, IEnvironment environment, object initialValue);

        public virtual IMap<string, INode> InitializeChildNodes(INode node, object snapshot)
        {
            return null;
        }

        public abstract string Describe { get; }

        protected abstract IValidationError[] IsValidSnapshot(object value, IContextEntry[] context);

        protected abstract S GetDefaultSnapshot();

        public abstract void ApplyPatchLocally(INode node, string subpath, IJsonPatch patch);

        public abstract void ApplySnapshot(INode node, S snapshot);

        public abstract INode GetChildNode(INode node, string key);

        public abstract IReadOnlyCollection<INode> GetChildren(INode node);

        public abstract IType GetChildType(string key);

        public abstract S GetSnapshot(INode node, bool applyPostProcess);

        public abstract T GetValue(INode node);

        public abstract void RemoveChild(INode node, string subpath);

        public virtual bool IsAssignableFrom(IType type)
        {
            return type == this;
        }

        public IValidationError[] Validate(object value, IContextEntry[] context)
        {
            if (value.IsStateTreeNode())
            {
                var node = value.GetStateTreeNode();

                var type = node.Type;
                if (type == this || IsAssignableFrom(type))
                {
                    return new IValidationError[] { };
                }

                return new IValidationError[] { new ValidationError { Context = context, Value = value } };
            }

            return IsValidSnapshot(value, context);
        }

        public bool Is(object value)
        {
            return Validate(value, new IContextEntry[] { new ContextEntry { Path = "", Type = this as IType } }).Length == 0;
        }


        public virtual INode Reconcile(INode current, object newValue)
        {
            if (current.Snapshot == newValue)
            {
                // newValue is the current snapshot of the node, noop
                return current;
            }

            if (newValue.IsStateTreeNode() && newValue.GetStateTreeNode() == current)
            {
                // the current node is the same as the new one
                return current;
            }

            if (
                current.Type == this &&
                // isMutable(newValue) &&
                !newValue.IsStateTreeNode() && (!(current is ObjectNode) || !(newValue is IDictionary<object, object>) ||
                string.IsNullOrWhiteSpace((current as ObjectNode).IdentifierAttribute) ||
               (string.Equals((current as ObjectNode).Identifier, (newValue as IDictionary<object, object>)[(current as ObjectNode).IdentifierAttribute])))
             )
            {
                // the newValue has no node, so can be treated like a snapshot
                // we can reconcile
                (current as ObjectNode).ApplySnapshot(newValue);

                return current;
            }

            var parent = current.Parent;
            var subpath = current.Subpath;
            var environment = current.Environment;

            // current node cannot be recycled in any way
            current.Dispose();

            // attempt to reuse the new one
            if (newValue.IsStateTreeNode() && IsAssignableFrom(newValue.GetNodeType()))
            {
                // newValue is a Node as well, move it here..
                var newNode = newValue.GetStateTreeNode();

                newNode.SetParent(parent, subpath);

                return newNode;
            }

            // nothing to do, we have to create a new node
            return Instantiate(parent, subpath, environment, newValue);
        }

        public object Create(object snapshot, IEnvironment environment)
        {
            return Create((S)snapshot, environment);
        }

        object IType.GetValue(INode node)
        {
            return GetValue(node);
        }

        object IType.GetSnapshot(INode node, bool applyPostProcess)
        {
            return GetSnapshot(node, applyPostProcess);
        }

        public virtual void ApplySnapshot(INode node, object snapshot)
        {
            ApplySnapshot(node, (S)snapshot);
        }

        IType IType.GetChildType(string key)
        {
            return GetChildType(key);
        }

        public T Type { get => throw new Exception("Factory.Type should not be actually called. It is just a Type signature that can be used at compile time with Typescript, by using `typeof type.Type`"); }

        public S SnapshotType { get => throw new Exception("Factory.SnapshotType should not be actually called. It is just a Type signature that can be used at compile time with Typescript, by using `typeof type.SnapshotType`"); }

        object IType.Type => Type;

        object IType.SnapshotType => SnapshotType;

        public static INode[] ConvertChildNodesToList(IMap<string, INode> childNodes)
        {
            if (childNodes == null || childNodes.Count == 0)
            {
                return Array.Empty<INode>();
            }

            return childNodes.Select(pair => pair.Value).ToArray();
        }
    }
}
