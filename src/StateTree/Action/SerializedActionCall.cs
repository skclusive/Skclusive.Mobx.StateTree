namespace Skclusive.Mobx.StateTree
{
    public class SerializedActionCall : ISerializedActionCall
    {
        public string Name { set; get; }

        public string Path { set; get; }

        public object[] Arguments { set; get; }
    }
}
