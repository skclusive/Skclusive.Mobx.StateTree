using Skclusive.Mobx.Observable;
using Xunit;

namespace Skclusive.Mobx.StateTree.Tests
{
    public interface ICounterPrimitive
    {
        int Count { set; get; }
    }

    public interface ICounterActions
    {
        void Increment();
    }

    public interface ICounter : ICounterPrimitive, ICounterActions
    {
    }

    public interface ICounterSnapshot : ICounterPrimitive
    {
    }

    internal class CounterSnapshot : ICounterSnapshot
    {
        public int Count { set; get; }
    }

    internal class CounterProxy : ObservableProxy<ICounter, INode>, ICounter
    {
        public override ICounter Proxy => this;

        public CounterProxy(IObservableObject<ICounter, INode> target) : base(target)
        {
        }

        public int Count
        {
            get => Read<int>(nameof(Count));
            set => Write(nameof(Count), value);
        }

        public void Increment()
        {
            (Target as dynamic).Increment();
        }
    }

    public interface ICounterAuditPrimitive : ICounterPrimitive
    {
        int Called { set; get; }
    }

    public interface ICounterAudit : ICounterAuditPrimitive, ICounter
    {
    }

    public interface ICounterAuditSnapshot : ICounterAuditPrimitive, ICounterSnapshot
    {
    }

    internal class CounterAuditSnapshot : CounterSnapshot, ICounterAuditSnapshot
    {
        public int Called { set; get; }
    }

    internal class CounterAuditProxy : ObservableProxy<ICounterAudit, INode>, ICounterAudit
    {
        public override ICounterAudit Proxy => this;

        public CounterAuditProxy(IObservableObject<ICounterAudit, INode> target) : base(target)
        {
        }

        public int Called
        {
            get => Read<int>(nameof(Called));
            set => Write(nameof(Called), value);
        }

        public int Count
        {
            get => Read<int>(nameof(Count));
            set => Write(nameof(Count), value);
        }

        public void Increment()
        {
            (Target as dynamic).Increment();
        }
    }

    public partial class TestTypes
    {
        public readonly static IObjectType<ICounterSnapshot, ICounter>  CounterType = Types.
                      Object<ICounterSnapshot, ICounter>("Counter")
                     .Proxy(x => new CounterProxy(x))
                     .Snapshot(() => new CounterSnapshot())
                     .Mutable(o => o.Count, Types.Optional(Types.Int, 0))
                     .Action(o => o.Increment(), (o) => o.Count++);
    }
}
