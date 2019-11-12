namespace Skclusive.Mobx.StateTree
{
    public interface ICustomTypeOptions<S, T>
    {
        // Friendly name
        string Name { get; }

        // given a serialized value, how to turn it into the target type
        T FromSnapshot(S snapshot);

        // return the serialization of the current value
        S ToSnapshot(T value);

        // if true, this is a converted value, if false, it's a snapshot
        bool IsTargetType(object value);

        // a non empty string is assumed to be a validation error
        string Validate(S snapshot);
    }


    public class CustomType<S, T> : Type<S, T>
    {
        public CustomType(ICustomTypeOptions<S, T> options) : base(options.Name)
        {
            Options = options;

            Flags = TypeFlags.Reference;

            ShouldAttachNode = false;
        }

        private ICustomTypeOptions<S, T> Options { set; get; }

        public override string Describe => Name;

        public override bool IsAssignableFrom(IType type)
        {
            return this == type;
        }

        public override INode Instantiate(INode parent, string subpath, IEnvironment environment, object initialValue)
        {
            var snapshot = Options.IsTargetType(initialValue) ? initialValue : Options.FromSnapshot((S)initialValue);

            return this.CreateNode(parent as ObjectNode, subpath, environment, snapshot);
        }

        protected override IValidationError[] IsValidSnapshot(object value, IContextEntry[] context)
        {
            if (Options.IsTargetType(value))
            {
                return new IValidationError[] { };
            }

            var error = Options.Validate((S)value);

            if (!string.IsNullOrWhiteSpace(error))
            {
                return new IValidationError[]
               {
                    new ValidationError
                    {
                        Context = context,

                        Value = value,

                        Message = $"Invalid value for type '${Name}': ${error}"
                    }
               };
            }

            return new IValidationError[] { };
        }

        public override T GetValue(INode node)
        {
            if (!node.IsAlive)
            {
                return default(T);
            }

            return (T)node.StoredValue;
        }

        public override S GetSnapshot(INode node, bool applyPostProcess)
        {
            return Options.ToSnapshot((T)node.StoredValue);
        }

        public override INode Reconcile(INode current, object newValue)
        {
            var isSnapshot = !Options.IsTargetType(newValue);

            var unchanged = current.Type == this && (isSnapshot ? newValue == current.Snapshot : newValue == current.StoredValue);

            if (unchanged)
            {
                return current;
            }

            var value = isSnapshot ? Options.FromSnapshot((S)newValue) : (T)newValue;

            var newnode = Instantiate(current.Parent, current.Subpath, current.Environment, value);

            current.Dispose();

            return newnode;
        }
    }
}
