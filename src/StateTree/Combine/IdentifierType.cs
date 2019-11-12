using System;
using System.Collections.Generic;
using System.Text;

namespace Skclusive.Mobx.StateTree
{
    public class IdentifierType<T> : Type<T, T>
    {
        public IdentifierType(IType<T, T> type) : base($"identifier(${type.Name})")
        {
            _Type = type;

            Flags = TypeFlags.Identifier;

            ShouldAttachNode = false;
        }

        private IType<T, T> _Type { set; get; }

        public override string Describe => $"identifier({_Type.Describe})";

        public override INode Instantiate(INode parent, string subpath, IEnvironment environment, object initialValue)
        {
            if (parent == null || !parent.StoredValue.IsStateTreeNode())
            {
                throw new InvalidOperationException($"Identifier types can only be instantiated as direct child of a model type");
            }

            if (parent is ObjectNode oparent)
            {
                if (!String.IsNullOrWhiteSpace(oparent.IdentifierAttribute))
                {
                    throw new InvalidOperationException($"Cannot define property '{subpath}' as object identifier, property '{oparent.IdentifierAttribute}' is already defined as identifier property");
                }

                oparent.IdentifierAttribute = subpath;
            }

            return this.CreateNode(parent as ObjectNode, subpath, environment, initialValue);
        }

        public override INode Reconcile(INode current, object newValue)
        {
            if (current.StoredValue != newValue)
            {
                throw new InvalidOperationException($"Tried to change identifier from '{current.StoredValue}' to '{newValue}'. Changing identifiers is not allowed.");
            }

            return current;
        }

        protected override IValidationError[] IsValidSnapshot(object value, IContextEntry[] context)
        {
            if (value == null || value is string || value is int)
            {
                return _Type.Validate(value, context);
            }

            return new IValidationError[]
            {
                new ValidationError
                {
                    Context = context,

                    Value = value,

                    Message = $"Value is not a valid identifier, which is a string or a number"
                }
            };
        }
    }
}
