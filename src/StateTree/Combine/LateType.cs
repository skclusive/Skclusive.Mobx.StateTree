using System;

namespace Skclusive.Mobx.StateTree
{
    public class LateType<S, T> : Type<S, T>
    {
        public LateType(string name, Func<IType<S, T>> definition) : base(name)
        {
            Definition = definition;
        }

        private Func<IType<S, T>> Definition { set; get; }

        private IType<S, T> _SubType;

        private IType<S, T> SubType
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


        public override string Describe => SubType.Describe;

        public override TypeFlags Flags => SubType.Flags | TypeFlags.Late;

        public override bool ShouldAttachNode => SubType.ShouldAttachNode;

        public override INode Instantiate(INode parent, string subpath, IEnvironment environment, object initialValue)
        {
            return SubType.Instantiate(parent, subpath, environment, initialValue);
        }

        public override INode Reconcile(INode current, object newValue)
        {
            return SubType.Reconcile(current, newValue);
        }

        public override bool IsAssignableFrom(IType type)
        {
            return SubType.IsAssignableFrom(type);
        }

        protected override IValidationError[] IsValidSnapshot(object value, IContextEntry[] context)
        {
            return SubType.Validate(value, context);
        }
    }
}
