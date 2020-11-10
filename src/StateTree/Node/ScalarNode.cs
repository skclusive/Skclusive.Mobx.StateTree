using Skclusive.Mobx.Observable;
using System;

namespace Skclusive.Mobx.StateTree
{
    public class ScalarNode : INode
    {
        public IEnvironment Environment { get; }

        protected NodeLifeCycle State { set; get; }

        public IType Type { get; }

        public object StoredValue { set; get; }

        public ObjectNode Parent { private set; get; }

        public string Subpath { set; get; }

        public bool AutoUnbox { get; }

        public ScalarNode(IType type,
           ObjectNode parent, string subpath,
           IEnvironment environment,
           object initialSnapshot,
           Func<object, IStateTreeNode, object> createNewInstance,
           Action<INode, object, IStateTreeNode> finalizeNewInstance = null)
        {
            Type = type;

            Parent = parent;

            Environment = environment;

            Subpath = subpath;

            IStateTreeNode meta = new StateTreeNode(this);

            StoredValue = createNewInstance(initialSnapshot, meta);

            AutoUnbox = true;

            State = NodeLifeCycle.INITIALIZING;

            bool sawException = true;
            try
            {
                finalizeNewInstance?.Invoke(this, initialSnapshot, meta);

                State = NodeLifeCycle.CREATED;

                sawException = false;
            }
            finally
            {
                if (sawException)
                {
                    // short-cut to die the instance, to avoid the snapshot computed starting to throw...
                    State = NodeLifeCycle.DEAD;
                }
            }
        }

        public string Path
        {
            get
            {
                if (Parent == null)
                {
                    return "";
                }

                return $"{Parent.Path}/{Subpath.EscapeJsonPath()}";
            }
        }

        public bool IsRoot => Parent == null;

        public ObjectNode Root
        {
            get
            {
                if (Parent == null)
                {
                    throw new Exception("This scalar node is not part of a tree");
                }
                return Parent.Root;
            }
        }

        public void SetParent(ObjectNode newParent, string subpath)
        {
           // throw new InvalidOperationException("Cannot change parent of immutable node");
        }

        public object Value => Type.GetValue(this);

        public object Snapshot => Type.GetSnapshot(this, false);

        public bool IsAlive => State != NodeLifeCycle.DEAD;

        public object Unbox(INode node)
        {
            if (node != null && node.AutoUnbox)
            {
                return node.Value;
            }
            return node;
        }

        public override string ToString()
        {
            var state = IsAlive ? "" : "[dead]";
            return $"{Type.Name}@{Path ?? "<root>"}{state}";
        }

        public void Dispose()
        {
            State = NodeLifeCycle.DEAD;
        }
    }
}
