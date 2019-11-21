// using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Skclusive.Mobx.StateTree
{
    public class LiteralType<T> : Type<T, T>, ISimpleType<T>
    {
        private T Value { set; get; }

        public LiteralType(T value) : base($"{value}") //base(JsonConvert.SerializeObject(value))
        {
            Flags = TypeFlags.Literal;

            ShouldAttachNode = false;

            Value = value;
        }

        public override string Describe => Name;

        public override INode Instantiate(INode parent, string subpath, IEnvironment environment, object initialValue)
        {
            return this.CreateNode(parent as ObjectNode, subpath, environment, initialValue);
        }

        protected override IValidationError[] IsValidSnapshot(object value, IContextEntry[] context)
        {
            if (value != null && (value is string || value.GetType().IsPrimitive || value.GetType().IsEnum) && EqualityComparer<object>.Default.Equals(value, Value))
            {
                return new IValidationError[] { };
            }

            return new IValidationError[]
            {
                new ValidationError
                {
                    Context = context,

                    Value = value,

                    Message = $"Value is not a literal {Name}"
                }
            };
        }
    }
}
