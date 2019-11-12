namespace Skclusive.Mobx.StateTree
{
    public interface IReversibleJsonPatch : IJsonPatch
    {
        object OldValue { set; get; }
    }
}
