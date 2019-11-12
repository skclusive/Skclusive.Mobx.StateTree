namespace Skclusive.Mobx.StateTree
{
    internal class ReversibleJsonPatch : JsonPatch, IReversibleJsonPatch
    {
        public object OldValue { get; set; }
    }
}
