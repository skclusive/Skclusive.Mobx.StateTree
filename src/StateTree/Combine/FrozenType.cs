using System;

namespace Skclusive.Mobx.StateTree
{
    public class FrozenType<T> : Type<T, T>, ISimpleType<T>
    {
        public FrozenType() : base("frozen")
        {
            Flags = TypeFlags.Frozen;

            ShouldAttachNode = false;
        }

        public override string Describe => "<any immutable value>";

        public override INode Instantiate(INode parent, string subpath, IEnvironment environment, object initialValue)
        {
            return this.CreateNode(parent as ObjectNode, subpath, environment, initialValue);
        }

        protected override IValidationError[] IsValidSnapshot(object value, IContextEntry[] context)
        {
            // TODO: to fix condition
            if (Math.Max(0, 1) == 0) // !isSerializable(value)
            {
                return new IValidationError[]
                {
                    new ValidationError
                    {
                        Context = context,

                        Value = value,

                        Message = $"Value is not serializable and cannot be frozen"
                    }
                };
            }

            return Array.Empty<IValidationError>();
        }
    }
}
