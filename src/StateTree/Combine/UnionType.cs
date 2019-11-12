using System;
using System.Collections.Generic;
using System.Linq;

namespace Skclusive.Mobx.StateTree
{
    public class UnionType<S, T> : Type<S, T>
    {
        private IList<IType> _Types { set; get; }

        private Func<object, IType> _Dispatcher { set; get; }

        public UnionType(string name, IList<IType> types, Func<object, IType> dispatcher) : base(name)
        {
            _Types = types;

            _Dispatcher = dispatcher;

            Flags = types.Aggregate(TypeFlags.Union, (acc, type) => acc | type.Flags);

            ShouldAttachNode = types.Any(type => type.ShouldAttachNode);
        }

        public override bool IsAssignableFrom(IType type)
        {
            return _Types.Any(subtype => subtype.IsAssignableFrom(type));
        }

        public override string Describe => $"({string.Join(" | ", _Types.Select(type => type.Describe))})";

        public override INode Instantiate(INode parent, string subpath, IEnvironment environment, object initialValue)
        {
            return DetermineType(initialValue).Instantiate(parent, subpath, environment, initialValue);
        }

        public override INode Reconcile(INode current, object newValue)
        {
            return DetermineType(newValue).Reconcile(current, newValue);
        }

        private IType DetermineType(object value)
        {
            // try the dispatcher, if defined
            if (_Dispatcher != null)
            {
                return _Dispatcher(value);
            }
            // find the most accomodating type
            var applicableTypes = _Types.Where(type => type.Is(value)).ToList();

            if (applicableTypes.Count > 1)
            {
                throw new Exception(
                $"Ambiguos snapshot {value} for union {Name}. Please provide a dispatch in the union declaration.");
            }

            return applicableTypes[0];
        }

        protected override IValidationError[] IsValidSnapshot(object value, IContextEntry[] context)
        {
            if (_Dispatcher != null)
            {
                return _Dispatcher(value).Validate(value, context);
            }

            var errors = _Types.Select(type => type.Validate(value, context));
            var applicableTypes = errors.Where(error => error.Length == 0).ToArray();

            if (applicableTypes.Length > 1)
            {
                return new IValidationError[]
                {
                    new ValidationError
                    {
                        Context = context,

                        Value = value,

                        Message = $"Multiple types are applicable for the union (hint: provide a dispatch function)"
                    }
                };
            }
            else if (applicableTypes.Length == 0)
            {
                return errors.Aggregate(new IValidationError[]
                {
                    new ValidationError
                    {
                        Context = context,

                        Value = value,

                        Message = $"No type is applicable for the union."
                    }
                }, (acc, error) => acc.Concat(error).ToArray());
            }

            return new IValidationError[] { };
        }
    }
}
