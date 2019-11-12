namespace Skclusive.Mobx.StateTree
{
    internal class TempNode : INode
    {
        public IType Type { set; get; }

        public object Value { set; get; }

        public object Snapshot { set; get; }

        public object StoredValue { set; get; }

        public string Path { set; get; }

        public bool IsRoot { set; get; }

        public ObjectNode Parent { set; get; }

        public ObjectNode Root { set; get; }

        public IEnvironment Environment { set; get; }

        public string Subpath { set; get; }

        public bool IsAlive { set; get; }

        public bool AutoUnbox { set; get; }

        public void Dispose()
        {
        }

        public void SetParent(ObjectNode newParent, string subpath)
        {
        }
    }
}
