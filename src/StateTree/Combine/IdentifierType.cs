using System;
using System.Collections.Generic;
using System.Text;

namespace Skclusive.Mobx.StateTree
{
    public class IdentifierType<T> : Type<T, T>
    {
        public IdentifierType(string name, IType<T, T> type) : base(name)
        {
            _Type = type;

            Flags = TypeFlags.Identifier;

            ShouldAttachNode = false;
        }

        private IType<T, T> _Type { set; get; }

        public override string Describe => Name;

        public override INode Instantiate(INode parent, string subpath, IEnvironment environment, object initialValue)
        {
            if (parent == null || !(parent.Type.Flags == TypeFlags.Object))
            {
                throw new InvalidOperationException($"{Name} can only be instantiated as direct child of a model type");
            }

            //if (parent is ObjectNode oparent)
            //{
            //    if (!String.IsNullOrWhiteSpace(oparent.IdentifierAttribute))
            //    {
            //        throw new InvalidOperationException($"Cannot define property '{subpath}' as object identifier, property '{oparent.IdentifierAttribute}' is already defined as identifier property");
            //    }

            //    oparent.IdentifierAttribute = subpath;
            //}

            return this.CreateNode(parent as ObjectNode, subpath, environment, initialValue);
        }

        public override INode Reconcile(INode current, object newValue)
        {
            if (current.StoredValue != newValue)
            {
                throw new InvalidOperationException($"Tried to change {Name} from '{current.StoredValue}' to '{newValue}'. Changing identifiers is not allowed.");
            }

            return current;
        }

        protected override IValidationError[] IsValidSnapshot(object value, IContextEntry[] context)
        {
            if (value == null || value.GetType() == typeof (T))
            {
                return _Type.Validate(value, context);
            }

            return new IValidationError[]
            {
                new ValidationError
                {
                    Context = context,

                    Value = value,

                    Message = $"Value is not a valid {Name}, which should be {typeof(T).Name}"
                }
            };
        }
    }

    public class StringIdentifierType : IdentifierType<string>
    {
        public StringIdentifierType() : base("identifier", new StringType())
        {
        }
    }

    public class IntIdentifierType : IdentifierType<int>
    {
        public IntIdentifierType() : base("identifierInt", new IntType())
        {
        }
    }
}
