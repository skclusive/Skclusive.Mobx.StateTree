using Skclusive.Mobx.Observable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Skclusive.Mobx.StateTree
{
    public class ListType<S, T> : ComplexType<S[], IObservableList<INode, T>>, IManipulator<INode, T> // where T : IStateTreeNode, ISnapshottable<S>
    {
        public ListType(string name, IType<S, T> subType) : base(name)
        {
            ShouldAttachNode = true;

            Flags = TypeFlags.List;

            SubType = subType;
        }

        private ObjectNode Node { set; get; }

        private IType<S, T> SubType { set; get; }

        public override string Describe => $"{SubType.Describe}[]";

        private IObservableList<INode, T> CreateNewInstance(IMap<string, INode> childNodes)
        {
            return ObservableList<INode, T>.FromIn(ConvertChildNodesToList(childNodes), null, this);
        }

        private void FinalizeNewInstance(ObjectNode node)
        {
            var instance = GetValue(node);

            Node = node;

            instance.Intercept(change => WillChange(change));

            instance.Observe(change => DidChange(change));
        }

        public override INode Instantiate(INode parent, string subpath, IEnvironment environment, object initialValue)
        {
            return this.CreateNode(parent as ObjectNode, subpath, environment, initialValue, (childNodes) => CreateNewInstance(childNodes as IMap<string, INode>), (node, snapshot) => FinalizeNewInstance(node as ObjectNode));
        }

        public override IMap<string, INode> InitializeChildNodes(INode node, object snapshot)
        {
            List<object> values = new List<object>();

            foreach(var item in ((IEnumerable)snapshot))
            {
                values.Add(item);
            }

            IEnvironment env = node.Environment;

            return values.Select((value, index) => (value, index)).Aggregate(new Map<string, INode>(), (map, pair) =>
            {
                var subpath = $"{pair.index}";

                map[subpath] = SubType.Instantiate(node, subpath, env, pair.value);

                return map;
            });
        }

        public override void ApplyPatchLocally(INode node, string subpath, IJsonPatch patch)
        {
            var value = GetValue(node);

            var index = subpath == "-" ? value.Length : int.Parse(subpath);

            switch (patch.Operation)
            {
                case JsonPatchOperation.Replace:
                    value[index] = (T)patch.Value;
                    break;

                case JsonPatchOperation.Add:
                    value.Splice(index, 0, (T)patch.Value);
                    break;

                case JsonPatchOperation.Remove:
                    value.RemoveAt(index);
                    break;
            }
        }

        // Custom Method
        public override void ApplySnapshot(INode node, object snapshot)
        {
            //if (snapshot is IList<T> list)
            //{
            //    snapshot = list.Select(item => item.IsStateTreeNode() ? item.GetSnapshot<S>() : (S)(object)item).ToArray();
            //}
            base.ApplySnapshot(node, snapshot);
        }

        public override void ApplySnapshot(INode node, S[] snapshot)
        {
            StateTreeUtils.Typecheck(this, snapshot);

            var values = snapshot.Select(snap => SubType.Create(snap, Node.Environment)).ToArray();

            GetValue(node).Replace(values);
        }

        public override INode GetChildNode(INode node, string key)
        {
            if (int.TryParse(key, out int index))
            {
                return GetValue(node).Get(index);
            }

            throw new InvalidOperationException($"Not a valid key {key}");
        }

        public override IReadOnlyCollection<INode> GetChildren(INode node)
        {
            return GetValue(node).GetValues().ToList();
        }

        public override IType GetChildType(string key)
        {
            return SubType;
        }

        public override S[] GetSnapshot(INode node, bool applyPostProcess)
        {
            return GetChildren(node).Select(item => (S)item.Snapshot).ToArray();
        }

        public override IObservableList<INode, T> GetValue(INode node)
        {
            return node.StoredValue as IObservableList<INode, T>;
        }

        public override void RemoveChild(INode node, string subpath)
        {
            if (int.TryParse(subpath, out int index))
            {
                GetValue(node).Splice(index, 1);

                return;
            }

            throw new Exception($"Not valid List Index {subpath}");
        }

        protected override S[] GetDefaultSnapshot()
        {
            return Array.Empty<S>();
        }

        protected override IValidationError[] IsValidSnapshot(object values, IContextEntry[] context)
        {
            if (values is IEnumerable enumerable)
            {
                IList<object> list = new List<object>();

                foreach(var item in enumerable)
                {
                    list.Add(item);
                }

                var errors = list.Select((value, index) => SubType.Validate(value, StateTreeUtils.GetContextForPath(context, $"{index}", SubType)));

                return errors.Aggregate(Array.Empty<IValidationError>(), (acc, value) => acc.Concat(value).ToArray());
            }

            return new IValidationError[]
            {
                new ValidationError
                {
                    Context = context,

                    Value = values,

                    Message = $"Value is not an array or list or enumerable"
                }
            };
        }


        private IListWillChange<INode> WillChange(IListWillChange<INode> change)
        {
            var node = change.Object.GetStateTreeNode();

            node.AssertWritable();

            var childNodes = node.GetChildren().ToList();

            switch (change.Type)
            {
                case ChangeType.UPDATE:
                    {
                        if (change.NewValue == childNodes[change.Index])
                        {
                            return null;
                        }
                        change.NewValue = ReconcileListItems(node, SubType,
                            new INode[] { childNodes[change.Index] }.ToList(),
                            new object[] { change.NewValue },
                            new string[] { $"{change.Index}" })[0];
                    }
                    break;
                case ChangeType.SPLICE:
                    {
                        change.Added = ReconcileListItems(node, SubType,
                            childNodes.Slice(change.Index, change.Index + change.RemovedCount),
                            change.Added,
                            change.Added.Select((added, index) => $"{change.Index + index}").ToArray())
                            .ToArray();

                        // update paths of remaining items
                        for (int i = change.Index + change.RemovedCount; i < childNodes.Count; i++)
                        {
                            childNodes[i].SetParent(node, $"{i + change.Added.Length - change.RemovedCount}");
                        }
                    }
                    break;
            }

            return change;
        }

        private void DidChange(IListDidChange<INode> change)
        {
            var node = change.Object.GetStateTreeNode();

            switch (change.Type)
            {
                case ChangeType.UPDATE:

                    node.Emitpatch(new ReversibleJsonPatch
                    {
                        Operation = JsonPatchOperation.Replace,

                        Path = $"{change.Index}",

                        Value = change.NewValue.Snapshot,

                        OldValue = change.OldValue?.Snapshot

                    }, node);

                    break;
                case ChangeType.SPLICE:

                    for (int i = change.RemovedCount - 1; i >= 0; i--)
                    {
                        node.Emitpatch(new ReversibleJsonPatch
                        {
                            Operation = JsonPatchOperation.Remove,

                            Path = $"{change.Index + i}",

                            OldValue = change.Removed[i].Snapshot

                        }, node);
                    }

                    for (int i = 0; i < change.AddedCount; i++)
                    {
                        node.Emitpatch(new ReversibleJsonPatch
                        {
                            Operation = JsonPatchOperation.Add,

                            Path = $"{change.Index + i}",

                            Value = node.GetChildNode($"{change.Index + i}").Snapshot

                        }, node);
                    }

                    break;
            }
        }


        public INode Enhance(INode newv, INode oldV, object name)
        {
            return newv;
        }

        public T Dehance(INode node)
        {
            return (T)(Node?.Unbox(node) ?? node.Value);
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

        public static bool AreSame(INode oldNode, object newValue)
        {
            // the new value has the same node
            if (newValue.IsStateTreeNode())
            {
                return newValue.GetStateTreeNode() == oldNode;
            }

            if (StateTreeUtils.IsMutatble(newValue) && oldNode.Snapshot == newValue)
            {
                return true;
            }

            if (oldNode is ObjectNode oNode)
            {
                if (!string.IsNullOrWhiteSpace(oNode.Identifier) && !string.IsNullOrWhiteSpace(oNode.IdentifierAttribute))
                {
                    return EqualityComparer<object>.Default.Equals(StateTreeUtils.GetPropertyValue(newValue, oNode.IdentifierAttribute), oNode.Identifier);
                }
            }

            return false;
        }

        public static INode ValueAsNode<Sx, Tx>(IType<Sx, Tx> type, ObjectNode parent, string subpath, object value)
        {
            return ValueAsNode(type, parent, subpath, value, null);
        }

        public static INode ValueAsNode<Sx, Tx>(IType<Sx, Tx> type, ObjectNode parent, string subpath, object value, INode oldNode)
        {
            StateTreeUtils.Typecheck(type, value);

            // the new value has a MST node
            if (value.IsStateTreeNode())
            {
                var node = value.GetStateTreeNode();

                node.AssertAlive();

                // the node lives here
                if (node.Parent != null && node.Parent == parent)
                {
                    node.SetParent(parent, subpath);

                    if (oldNode != null && oldNode != node)
                    {
                        oldNode.Dispose();
                    }
                    return node;
                }
            }

            // there is old node and new one is a value/snapshot
            if (oldNode != null)
            {
                var node = type.Reconcile(oldNode, value);
                node.SetParent(parent, subpath);
                return node;
            }

            // nothing to do, create from scratch
            return type.Instantiate(parent, subpath, parent.Environment, value);
        }

        public static List<INode> ReconcileListItems<Sx, Tx>(ObjectNode parent, IType<Sx, Tx> type,  List<INode> oldNodes, object[] newValues, string[] newPaths)
        {
            INode oldNode, oldMatch;

            object newValue;

            bool hasNewNode = false;

            for (int i = 0; ; i++)
            {
                hasNewNode = i <= newValues.Length - 1;
                oldNode = i < oldNodes.Count ? oldNodes[i] : null;
                newValue = hasNewNode ? newValues[i] : null;

                // for some reason, instead of newValue we got a node, fallback to the storedValue
                // TODO: https://github.com/mobxjs/mobx-state-tree/issues/340#issuecomment-325581681
                if (StateTreeUtils.IsNode(newValue) || newValue is TempNode)
                {
                    newValue = (newValue as INode).StoredValue;
                }

                // both are empty, end
                if (oldNode == null && !hasNewNode)
                {
                    break;
                    // new one does not exists, old one dies
                }
                else if (!hasNewNode)
                {
                    oldNode.Dispose();
                    oldNodes.Splice(i, 1);
                    i--;
                    // there is no old node, create it
                }
                else if (oldNode == null)
                {
                    // check if already belongs to the same parent. if so, avoid pushing item in. only swapping can occur.
                    if (newValue.IsStateTreeNode() && newValue.GetStateTreeNode().Parent == parent)
                    {
                        // this node is owned by this parent, but not in the reconcilable set, so it must be double
                        throw new Exception($"Cannot add an object to a state tree if it is already part of the same or another state tree.Tried to assign an object to '{parent.Path}/{newPaths[i]}', but it lives already at '{newValue.GetStateTreeNode().Path}'");
                    }
                    oldNodes.Splice(i, 0, ValueAsNode(type, parent, newPaths[i], newValue));

                }
                else if (AreSame(oldNode, newValue)) // both are the same, reconcile
                {
                    oldNodes[i] = ValueAsNode(type, parent, newPaths[i], newValue, oldNode);
                    // nothing to do, try to reorder
                }
                else
                {
                    oldMatch = null;

                    // find a possible candidate to reuse
                    for (int j = i; j < oldNodes.Count; j++)
                    {
                        if (AreSame(oldNodes[j], newValue))
                        {
                            oldMatch = oldNodes.Splice(j, 1)[0];
                            break;
                        }
                    }

                    oldNodes.Splice(i, 0, ValueAsNode(type, parent, newPaths[i], newValue, oldMatch));
                }
            }

            return oldNodes;
        }
    }
}
