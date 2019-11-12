namespace Skclusive.Mobx.StateTree
{
    public interface IMiddlewareEvent
    {
        MiddlewareEventType Type { set; get; }

        string Name { set; get; }

        int Id { set; get; }

        int ParentId { set; get; }

        int RootId { set; get; }

        object Target { set; get; }

        IStateTreeNode Context { set; get; }

        IStateTreeNode Tree { set; get; }

        object[] Arguments { set; get; }
    }
}
