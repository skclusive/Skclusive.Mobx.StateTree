using Skclusive.Mobx.Observable;
using System;
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

        private IObservableList<INode, T> CreateNewInstance()
        {
            return ObservableList<INode, T>.From(null, null, this);
        }

        private void FinalizeNewInstance(ObjectNode node, object snapshot)
        {
            var instance = GetValue(node);

            Node = node;

            instance.Intercept(change => WillChange(change));

            node.ApplySnapshot(snapshot);

            instance.Observe(change => DidChange(change));
        }

        public override INode Instantiate(INode parent, string subpath, IEnvironment environment, object initialValue)
        {
            return this.CreateNode(parent as ObjectNode, subpath, environment, initialValue, (_) => CreateNewInstance(), (node, snapshot) => FinalizeNewInstance(node as ObjectNode, snapshot));
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
            return new S[] { };
        }

        protected override IValidationError[] IsValidSnapshot(object values, IContextEntry[] context)
        {
            if (values is IEnumerable<object> enumerable)
            {
                var errors = enumerable.Select((value, index) => SubType.Validate(value, StateTreeUtils.GetContextForPath(context, $"{index}", SubType)));

                return errors.Aggregate(new IValidationError[] { }, (acc, value) => acc.Concat(value).ToArray());
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
                        change.NewValue = StateTreeUtils.ReconcileListItems(SubType, node,
                            new INode[] { childNodes[change.Index] }.ToList(),
                            new object[] { change.NewValue },
                            new string[] { $"{change.Index}" })[0];
                    }
                    break;
                case ChangeType.SPLICE:
                    {
                        change.Added = StateTreeUtils.ReconcileListItems(SubType, node,
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
    }
}
