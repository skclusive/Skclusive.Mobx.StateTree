using System;

namespace Skclusive.Mobx.StateTree
{
    // TODO: implement CoreType using types.custom ?
    public class CoreType<S, T> : Type<S, T>
    {
        public CoreType(string name, TypeFlags flags,
            Func<object, bool> checker,
            Func<object, IStateTreeNode, object> initializer = null) : base(name)
        {
            ShouldAttachNode = false;

            Flags = flags;

            Checker = checker;

            Initializer = initializer ?? ((value, meta) => value);
        }

        public Func<object, bool> Checker { get; private set; }

        public Func<object, IStateTreeNode, object> Initializer { get; private set; }

        public override string Describe { get => Name; }

        public override INode Instantiate(INode parent, string subpath, IEnvironment environment, object snapshot)
        {
            return this.CreateNode(parent as ObjectNode, subpath, environment, snapshot, Initializer);
        }

        protected override IValidationError[] IsValidSnapshot(object value, IContextEntry[] context)
        {
            if (value.IsSimpleType() && Checker.Invoke(value))
            {
                return new IValidationError[] { };
            }

            //var typeName = this.name === "Date" ? "Date or a unix milliseconds timestamp" : this.name

            return new IValidationError[]
            {
                new ValidationError
                {
                    Context = context,

                    Value = value,

                    Message = $"Value is not a {Name}"
                }
            };
        }
    }
}
