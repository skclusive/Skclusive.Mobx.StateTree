namespace Skclusive.Mobx.StateTree
{
    public class MiddlewareEvent : IMiddlewareEvent
    {
        public MiddlewareEventType Type { get; set; }

        public string Name { get; set; }

        public int Id { get; set; }

        public int ParentId { get; set; }

        public int RootId { get; set; }

        public object Target { set; get; }

        public IStateTreeNode Context { get; set; }

        public IStateTreeNode Tree { get; set; }

        public object[] Arguments { get; set; }
    }
}
