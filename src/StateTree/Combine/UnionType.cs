using System;
using System.Collections.Generic;
using System.Linq;

namespace Skclusive.Mobx.StateTree
{
    public class UnionOptions
    {
        public Func<object, IType> Dispatcher { set; get; }

        public bool Eager { set; get; }
    }

    public class UnionType<S, T> : Type<S, T>, IUnionType
    {
        private IList<IType> _Types { set; get; }

        private Func<object, IType> _Dispatcher { set; get; }

        private bool _Eager { set; get; }

        public UnionType(string name, IList<IType> types, UnionOptions options = null) : base(name)
        {
            _Types = types;

            _Dispatcher = options?.Dispatcher;

            _Eager = options?.Eager ?? true;

            Flags = types.Aggregate(TypeFlags.Union, (acc, type) => acc | type.Flags);

            ShouldAttachNode = types.Any(type => type.ShouldAttachNode);
        }

        public override bool IsAssignableFrom(IType type)
        {
            return _Types.Any(subtype => subtype.IsAssignableFrom(type));
        }

        public override string Describe => $"({string.Join(" | ", _Types.Select(type => type.Describe))})";

        IType[] IUnionType.SubTypes => _Types.ToArray();

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
            return _Types.FirstOrDefault(type => type.Is(value));
        }

        protected override IValidationError[] IsValidSnapshot(object value, IContextEntry[] context)
        {
            if (_Dispatcher != null)
            {
                return _Dispatcher(value).Validate(value, context);
            }

            var allErrors = new List<IValidationError[]>();

            var applicableTypes = 0;

            for (var i = 0; i < _Types.Count; i++)
            {
                var type = _Types[i];

                var errors = type.Validate(value, context);

                if (errors.Length == 0)
                {
                    if (_Eager)
                    {
                        return Array.Empty<IValidationError>();
                    }
                    else
                    {
                        applicableTypes++;
                    }
                }
                else
                {
                    allErrors.Add(errors);
                }
            }

            if (applicableTypes == 1)
            {
                return Array.Empty<IValidationError>();
            }

            return allErrors.Aggregate(new IValidationError[]
            {
                new ValidationError
                {
                    Context = context,

                    Value = value,

                    Message = $"No type is applicable for the union."
                }
            }, (acc, error) => acc.Concat(error).ToArray());
        }
    }
}
