using System;
using System.Collections.Generic;
using System.Text;

namespace Skclusive.Mobx.StateTree
{
    public enum StoreType
    {
        Object,

        Identifier
    }

    public class StoredReference
    {
        public StoreType Type { private set; get; }

        public object Value { private set; get; }

        internal StoredReference(StoreType type, object value)
        {
            Type = type;

            Value = value;

            if (Type == StoreType.Object)
            {
                if (!value.IsStateTreeNode())
                {
                    throw new ArgumentException($"Can only store references to tree nodes, got: '{value}'");
                }

                var node = value.GetStateTreeNode();

                if (string.IsNullOrWhiteSpace(node.IdentifierAttribute))
                {
                    throw new ArgumentException("Can only store references with a defined identifier attribute.");
                }
            }
        }
    }

    public abstract class BaseReferenceType<I, S, T> : Type<I, T>
    {
        protected BaseReferenceType(IType<S, T> targetType) : base($"reference(${ targetType.Name})")
        {
            TargetType = targetType;

            Flags = TypeFlags.Reference;

            ShouldAttachNode = false;
        }

        protected IType<S, T> TargetType { set; get; }

        public override string Describe => Name;

        public override bool IsAssignableFrom(IType type)
        {
            return TargetType.IsAssignableFrom(type);
        }

        protected override IValidationError[] IsValidSnapshot(object value, IContextEntry[] context)
        {
            if (typeof(I) != value.GetType())
            {
               return new IValidationError[]
               {
                    new ValidationError
                    {
                        Context = context,

                        Value = value,

                        Message = $"Value is not a valid identifier, which is a {typeof(S).Name}"
                    }
               };
            }

            return new IValidationError[] { };
        }
    }

    public class IdentifierReferenceType<I, S, T> : BaseReferenceType<I, S, T>
    {
        public IdentifierReferenceType(IType<S, T> targetType) : base(targetType)
        {
        }

        public override T GetValue(INode node)
        {
            if (!node.IsAlive)
            {
                return default;
            }

            var storeRef = node.StoredValue as StoredReference;

            // id already resolved, return
            if (storeRef.Type == StoreType.Object)
            {
                return (T)storeRef.Value;
            }

            // reference was initialized with the identifier of the target
            var target = node.Root.IdentifierCache?.Resolve(TargetType, (string)storeRef.Value);

            if (target == null)
            {
                throw new Exception($"Failed to resolve reference '{storeRef.Value}' to type '{TargetType.Name}' (from node: {node.Path})");
            }

            return (T)target.Value;
        }

        public override I GetSnapshot(INode node, bool applyPostProcess)
        {
            var storeRef = node.StoredValue as StoredReference;

            switch (storeRef.Type)
            {
                case StoreType.Object:
                    return (I)(object)storeRef.Value.GetStateTreeNode().Identifier;
                case StoreType.Identifier:
                    return (I)storeRef.Value;
            }

            throw new Exception("Failed to get snapshot");
        }

        public override INode Instantiate(INode parent, string subpath, IEnvironment environment, object snapshot)
        {
            return this.CreateNode<string, StoredReference>(parent as ObjectNode, subpath, environment, new StoredReference(snapshot.IsStateTreeNode() ? StoreType.Object : StoreType.Identifier, snapshot));
        }

        public override INode Reconcile(INode current, object newValue)
        {
            if (current.Type == this)
            {
                var targetMode = newValue.IsStateTreeNode() ? StoreType.Object : StoreType.Identifier;

                var storeRef = current.StoredValue as StoredReference;

                if (targetMode == storeRef.Type && storeRef.Value == newValue)
                {
                    return current;
                }
            }

            var newNode = Instantiate(current.Parent, current.Subpath, current.Environment, newValue);

            current.Dispose();

            return newNode;
        }
    }

    public interface IReferenceOptions<S, T>
    {
        T GetReference(S identifier, IStateTreeNode parent);

        S SetReference(T value, IStateTreeNode parent);
    }

    public class CustomReferenceType<S, T> : BaseReferenceType<S, S, T>
    {
        public CustomReferenceType(IType<S, T> targetType, IReferenceOptions<S, T> options) : base(targetType)
        {
            Options = options;
        }

        private IReferenceOptions<S, T> Options { set; get; }

        public override T GetValue(INode node)
        {
            if (!node.IsAlive)
            {
                return default(T);
            }
            return Options.GetReference((S)node.StoredValue, (IStateTreeNode)node.Parent?.StoredValue);
        }

        public override S GetSnapshot(INode node, bool applyPostProcess)
        {
            return (S)node.StoredValue;
        }

        public override INode Instantiate(INode parent, string subpath, IEnvironment environment, object snapshot)
        {
            var identifier = snapshot.IsStateTreeNode() ? Options.SetReference((T)snapshot, (IStateTreeNode)parent?.StoredValue) : snapshot;

            return this.CreateNode(parent as ObjectNode, subpath, environment, identifier);
        }

        public override INode Reconcile(INode current, object newValue)
        {
            var newIdentifier = newValue.IsStateTreeNode() ? Options.SetReference((T)newValue, (IStateTreeNode)current?.StoredValue) : newValue;

            if (current.Type == this && current.StoredValue == newIdentifier)
            {
                return current;
            }

            var newNode = Instantiate(current.Parent, current.Subpath, current.Environment, newIdentifier);

            current.Dispose();

            return newNode;
        }
    }
}
