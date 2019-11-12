namespace Skclusive.Mobx.StateTree
{
    public interface ISerializedActionCall
    {
        string Name { get; }

        string Path { get; }

        object [] Arguments { get; }
    }
}
