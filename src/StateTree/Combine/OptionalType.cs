using System;
using System.Collections.Generic;
using System.Text;

namespace Skclusive.Mobx.StateTree
{
    public class OptionalType<S, T> : Type<S, T>
    {
        private IType<S, T> _Type { set; get; }

        private Func<S> _DefaultValue { set; get; }

        public override string Describe => $"{_Type.Describe}?";

        public OptionalType(IType<S, T> type, Func<S> defaultValue) : base(type.Name)
        {
            _Type = type;

            _DefaultValue = defaultValue;

            Flags = type.Flags | TypeFlags.Optional;

            ShouldAttachNode = type.ShouldAttachNode;
        }

        private S GetDefaultValue()
        {
            var defaultValue = _DefaultValue != null ? _DefaultValue() : default(S);

            StateTreeUtils.Typecheck(_Type, defaultValue);

            return defaultValue;
        }


        public override INode Instantiate(INode parent, string subpath, IEnvironment environment, object initialValue)
        {
            var value = initialValue ?? GetDefaultValue();

            return _Type.Instantiate(parent, subpath, environment, value);
        }

        public override INode Reconcile(INode current, object newValue)
        {
            return _Type.Reconcile(current, _Type.Is(newValue) && newValue != null ? newValue : GetDefaultValue());
        }

        public override bool IsAssignableFrom(IType type)
        {
            return _Type.IsAssignableFrom(type);
        }

        protected override IValidationError[] IsValidSnapshot(object value, IContextEntry[] context)
        {
            // defaulted values can be skipped
            if (value == null)
            {
                return new IValidationError[] { };
            }
            // bounce validation to the sub-type
            return _Type.Validate(value, context);
        }
    }
}
