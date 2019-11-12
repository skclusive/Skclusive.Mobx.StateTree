namespace Skclusive.Mobx.StateTree
{
    public class ValidationError : IValidationError
    {
        public IContextEntry[] Context { get; set; }

        public object Value { get; set; }

        public string Message { get; set; }
    }
}
