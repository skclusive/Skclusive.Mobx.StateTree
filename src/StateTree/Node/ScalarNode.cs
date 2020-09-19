using Skclusive.Mobx.Observable;
using System;

namespace Skclusive.Mobx.StateTree
{
    public class ScalarNode : INode
    {
        public IEnvironment Environment { set; get; }

        private IObservableValue<string> _SubPath { set; get; }

        public string Subpath { set => (_SubPath as IValueWriter<string>).Value = value; get => (_SubPath as IValueReader<string>).Value; }

        public bool AutoUnbox { set; get; }

        public ScalarNode(IType type,
           ObjectNode parent, string subpath,
           IEnvironment environment,
           object initialValue, object storedValue,
           bool canAttachTreeNode,
           Action<INode, object> finalize)
        {
            Type = type;

            StoredValue = storedValue;

            Parent = parent;

            Environment = environment;

            _SubPath = ObservableValue<string>.From();

            Subpath = subpath;

            AutoUnbox = true;

            State = NodeLifeCycle.INITIALIZING;

            if (canAttachTreeNode)
            {
                NodeCache.Add(StoredValue, new StateTreeNode(this));
            }

            bool sawException = true;
            try
            {
                finalize?.Invoke(this, initialValue);

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

        protected NodeLifeCycle State { set; get; }

        public IType Type { set; get; }

        public object StoredValue { set; get; }

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

        public bool IsRoot { get => Parent == null; }

        public ObjectNode Parent { private set; get; }

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
            // TODO: custom changes
            if (Parent == null)
            {
                Parent = newParent;
            }

            if(Parent != newParent)
            {
                throw new InvalidOperationException("Cannot change parent of immutable node");
            }

            if(Subpath != subpath)
            {
                Subpath = subpath ?? "";
            }
        }

        public object Value { get => Type.GetValue(this); }

        public object Snapshot { get => Type.GetSnapshot(this, false);  }


        public bool IsAlive { get => State != NodeLifeCycle.DEAD; }

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
