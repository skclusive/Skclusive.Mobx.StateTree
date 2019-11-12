namespace Skclusive.Mobx.StateTree
{
    public class ContextEntry : IContextEntry
    {
        public string Path { get; set; }

        public IType Type { get; set; }
    }
}
