using Skclusive.Mobx.Observable;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Skclusive.Mobx.StateTree
{
    public class ObjectNode : INode
    {
        private static int NextNodeId = 1;

        public ObjectNode(IType type,
            ObjectNode parent, string subpath,
            IEnvironment environment,
            object initialValue, object storedValue,
            bool canAttachTreeNode,
            Action<INode, object> finalize)
        {
            NodeId = ++NextNodeId;

            Type = type;

            StoredValue = storedValue;

            _SubPath = ObservableValue<string>.From();

            Subpath = subpath;

            _Parent = ObservableValue<ObjectNode>.From();

            Parent = parent;

            Environment = environment;

            _IsRunningAction = false;

            IsProtectionEnabled = true;

            AutoUnbox = true;

            State = NodeLifeCycle.INITIALIZING;

            PreBoot();

            PreCompute();

            if (parent == null)
            {
                IdentifierCache = new IdentifierCache();
            }

            if (canAttachTreeNode)
            {
                NodeCache.Add(StoredValue, new StateTreeNode(this));
            }

            bool sawException = true;
            try
            {
                _IsRunningAction = true;

                finalize?.Invoke(this, initialValue);

                _IsRunningAction = false;

                if (parent != null)
                {
                    parent.Root.IdentifierCache?.AddNodeToCache(this);
                }
                else
                {
                    IdentifierCache?.AddNodeToCache(this);
                }

                FireHook("afterCreate");

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

            var disposer = Reactions.Reaction<object>((r) => Snapshot, (snapshot, r) => EmitSnapshot(snapshot));

            AddDisposer(() => disposer.Dispose());
        }

        private void PreCompute()
        {
            _Path = ComputedValue<string>.From(() =>
            {
                if (Parent == null)
                {
                    return "";
                }

                return $"{Parent.Path}/{Subpath.EscapeJsonPath()}";
            });

            _Value = ComputedValue<object>.From(() =>
            {
                if (!IsAlive)
                {
                    return null;
                }
                return Type.GetValue(this);
            });

            _Snapshot = ComputedValue<object>.From(() =>
            {
                if (!IsAlive)
                {
                    return null;
                }
                return Type.GetSnapshot(this, false);
            });
        }

        private void InvalidateComputedPath()
        {
            _Path.TrackAndCompute();
        }

        public string IdentifierAttribute { set; get; }

        public bool AutoUnbox { protected set; get; }

        protected NodeLifeCycle State { set; get; }

        public IList<IMiddleware> Middlewares { set; get; }

        protected IList<Action<object>> SnapshotSubscribers { set; get; }

        protected IList<Action> DisposerSubscribers { set; get; }

        protected IList<Action<IJsonPatch, IJsonPatch>> PatchSubscribers { set; get; }

        public bool IsProtectionEnabled { set; get; }

        internal bool _IsRunningAction { set; get; }

        private IObservableValue<string> _SubPath { set; get; }

        public string Subpath { set => (_SubPath as IValueWriter<string>).Value = value; get => (_SubPath as IValueReader<string>).Value; }

        private IObservableValue<ObjectNode> _Parent { set; get; }

        public ObjectNode Parent { protected set => (_Parent as IValueWriter<ObjectNode>).Value = value; get => (_Parent as IValueReader<ObjectNode>)?.Value; }

        public int NodeId { get; private set; }

        public IType Type { set; get; }

        // public IDictionary<string, object> StoredValue { set; get; }

        public object StoredValue { set; get; }

        private IComputedValue<string> _Path { set; get; }

        private IComputedValue<object> _Value { set; get; }

        private IComputedValue<object> _Snapshot { set; get; }

        public string Path { get => _Path.Value; }

        public bool IsRoot { get => Parent == null; }

        public ObjectNode Root
        {
            get
            {
                ObjectNode parent = null;
                ObjectNode root = this;
                while ((parent = root.Parent) != null)
                {
                    root = parent;
                }
                return root;
            }
        }

        private Action<object> _ApplySnapshot { get; set; }

        private Action<IJsonPatch[]> _ApplyPatches { get; set; }

        private void PreBoot()
        {
            Middlewares = new List<IMiddleware>();

            DisposerSubscribers = new List<Action>();

            SnapshotSubscribers = new List<Action<object>>();

            PatchSubscribers = new List<Action<IJsonPatch, IJsonPatch>>();

            _ApplyPatches = StateTreeAction.CreateActionInvoker<IJsonPatch[]>
            (
                StoredValue, "@APPLY_PATCHES",

                (IJsonPatch[] patches) =>
                {
                    foreach (var patch in patches)
                    {
                        var paths = patch.Path.SplitJsonPath();
                        var node = this.ResolveNodeByPaths(paths.Take(paths.Length - 1));
                        (node as ObjectNode).ApplyPatchLocally(paths[paths.Length - 1], patch);
                    }
                }
            );

            _ApplySnapshot = StateTreeAction.CreateActionInvoker<object>
            (
                StoredValue, "@APPLY_SNAPSHOT",

                (object snapshot) =>
                {
                    // if the snapshot is the same as the current one, avoid performing a reconcile
                    if (snapshot != Snapshot)
                    {
                        // apply it by calling the type logic
                        Type.ApplySnapshot(this, snapshot);
                    }
                }
           );
        }

        public void ApplySnapshot(object snapshot)
        {
            _ApplySnapshot.Invoke(snapshot);
        }

        public void ApplyPatches(IJsonPatch[] patches)
        {
            _ApplyPatches.Invoke(patches);
        }

        public IEnvironment Environment { set; get; }

        public bool IsAlive { get => State != NodeLifeCycle.DEAD; }

        public object Value { get => _Value.Value; }

        public object Snapshot
        {
            get
            {
                // TODO: added to fix bug. need to investigate
                _Snapshot.TrackAndCompute();

               return _Snapshot.Value;
            }
        }

        public string Identifier { get => IdentifierAttribute ?? (string)FromStored(IdentifierAttribute); }

        private object FromStored(string attribute)
        {
            if (StoredValue is IDictionary<object, object>)
            {
                return (StoredValue as IDictionary<object, object>)[attribute];
            }
            return null;
        }

        public IdentifierCache IdentifierCache { set; get; }

        public bool IsRunningAction
        {
            get
            {
                if (_IsRunningAction)
                {
                    return true;
                }
                if (IsRoot)
                {
                    return false;
                }
                return Parent.IsRunningAction;
            }
        }

        public void SetParent(ObjectNode newParent, string subpath)
        {
            var parent = Parent;

            if (parent == newParent && Subpath == subpath)
            {
                return;
            }

            if (newParent != null)
            {
                if (parent != null && newParent != parent)
                {
                    throw new Exception($"A node cannot exists twice in the state tree. Failed to add {this} to path '{newParent.Path}/{subpath}'.");
                }

                if (parent == null && newParent.Root == this)
                {
                    throw new Exception($"A state tree is not allowed to contain itself. Cannot assign {this} to path '{newParent.Path}/{subpath}'");
                }

                if (parent == null && Root.Environment != null && Root.Environment != newParent.Root.Environment)
                {
                    throw new Exception("A state tree cannot be made part of another state tree as long as their environments are different.");
                }
            }
            if (parent != null && newParent == null)
            {
                Dispose();
            }
            else
            {
                Subpath = subpath ?? "";

                if (newParent != null && newParent != parent)
                {
                    newParent.Root.IdentifierCache.MergeCache(this);

                    Parent = newParent;

                    FireHook("afterAttach");
                }

                InvalidateComputedPath();
            }
        }

        protected void FireHook(string name)
        {
            object action = FromStored(name);
            if (action is Func<object, object>)
            {
                (action as Func<object, object>).Invoke(StoredValue);
            }
            else if (action is Action<object>)
            {
                (action as Action<object>).Invoke(StoredValue);
            }
        }

        internal void AssertAlive()
        {
            if (!IsAlive)
            {
                throw new Exception($"You are trying to read or write to an object that is no longer part of a state tree. (Object type was '{Type.Name}').");
            }
        }

        public INode GetChildNode(string subpath)
        {
            AssertAlive();
            AutoUnbox = false;
            try
            {
                return Type.GetChildNode(this, subpath);
            }
            finally
            {
                AutoUnbox = true;
            }
        }

        public IReadOnlyCollection<INode> GetChildren()
        {
            AssertAlive();
            AutoUnbox = false;
            try
            {
                return Type.GetChildren(this);
            }
            finally
            {
                AutoUnbox = true;
            }
        }

        public IType GetChildType(string key)
        {
            return Type.GetChildType(key);
        }

        public bool IsProtected
        {
            get => Root.IsProtectionEnabled;
        }

        public void AssertWritable()
        {
            AssertAlive();

            if (!IsRunningAction && IsProtected)
            {
                throw new Exception($"Cannot modify '{this}', the object is protected and can only be modified by using an action.");
            }
        }

        public void RemoveChild(string subpath)
        {
            Type.RemoveChild(this, subpath);
        }

        public object Unbox(INode node)
        {
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

        public override string ToString()
        {
            var state = IsAlive ? "" : "[dead]";
            var identifier = string.IsNullOrWhiteSpace(Identifier) ? "" : $"(id: {Identifier})";
            return $"{Type.Name}@{Path ?? "<root>"}{identifier}{state}";
        }

        public void FinalizeCreation()
        {
            // goal: afterCreate hooks runs depth-first. After attach runs parent first, so on afterAttach the parent has completed already
            if (State == NodeLifeCycle.CREATED)
            {
                if (Parent != null)
                {
                    if (Parent.State != NodeLifeCycle.FINALIZED)
                    {
                        // parent not ready yet, postpone
                        return;
                    }
                    FireHook("afterAttach");
                }

                State = NodeLifeCycle.FINALIZED;

                foreach (var child in GetChildren())
                {
                    if (child is ObjectNode)
                    {
                        (child as ObjectNode).FinalizeCreation();
                    }
                }
            }
        }

        public void Detach()
        {
            if (!IsAlive)
            {
                throw new Exception("Error while detaching, node is not alive.");
            }

            if (!IsRoot)
            {
                FireHook("beforeDetach");

                Environment = Root.Environment; // make backup of environment

                State = NodeLifeCycle.DETACHING;

                IdentifierCache = Root.IdentifierCache?.SplitCache(this);

                Parent?.RemoveChild(Subpath);

                Parent = null;

                Subpath = "";

                State = NodeLifeCycle.FINALIZED;

                InvalidateComputedPath();
            }
        }

        public void Dispose()
        {
            if (State == NodeLifeCycle.DETACHING)
            {
                return;
            }

            if (StoredValue.IsStateTreeNode())
            {
                // optimization: don't use walk, but getChildNodes for more efficiency
                StateTreeUtils.Walk(StoredValue.GetStateTree(), child =>
                 {
                     var node = child.GetStateTreeNode();

                     node?.AboutToDie();
                 });

                StateTreeUtils.Walk(StoredValue.GetStateTree(), child =>
                {
                    var node = child.GetStateTreeNode();

                    node?.FinalizeDeath();
                });
            }
        }

        public void AboutToDie()
        {
            foreach (var disposer in DisposerSubscribers)
            {
                disposer();
            }

            FireHook("beforeDestroy");
        }

        public void FinalizeDeath()
        {
            Root.IdentifierCache?.NotifyDisposed(this);

            // var oldPath = Path;

            // addReadOnlyProp(this, "snapshot", this.snapshot);

            _Parent = null;

            _SubPath = null;

            PatchSubscribers.Clear();

            SnapshotSubscribers.Clear();

            DisposerSubscribers.Clear();

            State = NodeLifeCycle.DEAD;

            InvalidateComputedPath();
        }

        public IDisposable OnSnapshot(Action<object> onChange)
        {
            SnapshotSubscribers.Add(onChange);

            return new Disposable(() => SnapshotSubscribers.Remove(onChange));
        }

        protected void EmitSnapshot(object snapshot)
        {
            foreach (var subscriber in SnapshotSubscribers)
            {
                subscriber(snapshot);
            }
        }

        public IDisposable OnPatch(Action<IJsonPatch, IJsonPatch> onPatch)
        {
            PatchSubscribers.Add(onPatch);

            return new Disposable(() => PatchSubscribers.Remove(onPatch));
        }

        public void Emitpatch(IReversibleJsonPatch basePatch, INode source)
        {
            if (PatchSubscribers.Count > 0)
            {
                var localizedPatch = new ReversibleJsonPatch
                {
                    Operation = basePatch.Operation,

                    Value = basePatch.Value,

                    OldValue = basePatch.OldValue,

                    Path = $"{source.Path.Substring(Path.Length)}/{basePatch.Path}" // calculate the relative path of the patch
                };

                var (patch, reversePatch) = localizedPatch.SplitPatch();

                foreach (var subscriber in PatchSubscribers)
                {
                    subscriber(patch, reversePatch);
                }
            }

            Parent?.Emitpatch(basePatch, source);
        }

        public IDisposable AddDisposer(Action disposer)
        {
            DisposerSubscribers.Insert(0, disposer);

            return new Disposable(() => DisposerSubscribers.Remove(disposer));
        }

        public IDisposable AddMiddleware(Action<IMiddlewareEvent, Action<IMiddlewareEvent, Func<object, object>>, Action<object>> handler, bool includeHooks = true)
        {
            var middleware = new Middleware
            {
                Handler = handler,

                IncludeHooks = includeHooks
            };

            Middlewares.Add(middleware);

            return new Disposable(() => RemoveMiddleware(handler));
        }

        public void RemoveMiddleware(Action<IMiddlewareEvent, Action<IMiddlewareEvent, Func<object, object>>, Action<object>> handler)
        {
            Middlewares = Middlewares.Where(middleware => middleware.Handler != handler).ToList();
        }

        public void ApplyPatchLocally(string subpath, IJsonPatch patch)
        {
            AssertWritable();

            Type.ApplyPatchLocally(this, subpath, patch);
        }
    }
}
