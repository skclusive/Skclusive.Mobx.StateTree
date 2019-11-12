using System;
namespace Skclusive.Mobx.StateTree
{
    public class RefinementType<S, T> : Type<S, T>
    {
        public RefinementType(string name, IType<S, T> type, Func<S, bool> predicator, Func<S, string> validator) : base(name)
        {
            _Type = type;

            Predicator = predicator;

            Validator = validator;

            ShouldAttachNode = _Type.ShouldAttachNode;

            Flags = _Type.Flags | TypeFlags.Refinement;
        }

        private IType<S, T> _Type { set; get; }

        private Func<S, bool> Predicator { set; get; }

        private Func<S, string> Validator { set; get; }

        public override string Describe => Name;

        public override INode Instantiate(INode parent, string subpath, IEnvironment environment, object initialValue)
        {
            return _Type.Instantiate(parent, subpath, environment, initialValue);
        }

        public override bool IsAssignableFrom(IType type)
        {
            return _Type.IsAssignableFrom(type);
        }

        protected override IValidationError[] IsValidSnapshot(object value, IContextEntry[] context)
        {
            var errors = _Type.Validate(value, context);

            if (errors.Length > 0)
            {
                return errors;
            }

            var snapshot = value.IsStateTreeNode() ? value.GetStateTreeNode().Snapshot : value;

            if (!Predicator((S)snapshot))
            {
                return new IValidationError[]
                {
                    new ValidationError
                    {
                        Context = context,

                        Value = value,

                        Message = Validator((S)value)
                    }
                };
            }

            return new IValidationError[] { };
        }
    }
}
