namespace Skclusive.Mobx.StateTree
{
    public class JsonPatch : IJsonPatch
    {
        public JsonPatchOperation Operation { get; set; }

        public string Path { get; set; }

        public object Value { get; set; }
    }
}
