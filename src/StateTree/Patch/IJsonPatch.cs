namespace Skclusive.Mobx.StateTree
{

    public interface IJsonPatch
    {
        JsonPatchOperation Operation { set; get; }

        string Path { set; get; }

        object Value { set; get; }
    }
}
