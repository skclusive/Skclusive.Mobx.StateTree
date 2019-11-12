using System;
using System.Collections.Generic;
using System.Linq;

namespace Skclusive.Mobx.StateTree
{
    public abstract class Type<S, T> : ComplexType<S, T>, IType<S, T>
    {
        public Type(string name) : base(name)
        {
        }

        public override T GetValue(INode node)
        {
            return (T)node.StoredValue;
        }

        public override S GetSnapshot(INode node, bool applyPostProcess)
        {
            return (S)node.StoredValue;
        }

        public override void ApplySnapshot(INode node, S snapshot)
        {
            throw new InvalidOperationException("Immutable types do not support applying snapshots");
        }

        public override void ApplyPatchLocally(INode node, string subpath, IJsonPatch patch)
        {
            throw new InvalidOperationException("Immutable types do not support applying patches");
        }

        public override IReadOnlyCollection<INode> GetChildren(INode node)
        {
            return Enumerable.Empty<INode>().ToList();
        }

        public override INode GetChildNode(INode node, string key)
        {
            throw new InvalidOperationException($"No child '{key}' available in type: ${Name}");
        }

        public override IType GetChildType(string key)
        {
            throw new InvalidOperationException($"No child '{key}' available in type: {Name}");
        }

        protected override S GetDefaultSnapshot()
        {
            return default(S);
        }

        public override INode Reconcile(INode current, object newValue)
        {
            // reconcile only if type and value are still the same
            if (current.Type == this && current.StoredValue == newValue)
            {
                return current;
            }

            var result = Instantiate(current.Parent, current.Subpath, current.Environment, newValue);

            current.Dispose();

            return result;
        }

        public override void RemoveChild(INode node, string subpath)
        {
            throw new InvalidOperationException($"No child '${subpath}' available in type: ${Name}");
        }
    }
}
