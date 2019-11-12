using Skclusive.Mobx.Observable;
using Xunit;

namespace Skclusive.Mobx.StateTree.Tests
{

    #region ICounter

    public interface ICounterProps
    {
        int Count { set; get; }
    }

    public interface ICounterActions
    {
        void Increment();
    }

    public interface ICounter : ICounterProps, ICounterActions
    {
    }

    internal class CounterSnapshot : ICounterProps
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

    #endregion

    #region ICounterAudit

    public interface ICounterAuditProps : ICounterProps
    {
        int Called { set; get; }
    }

    public interface ICounterAudit : ICounterAuditProps, ICounter
    {
    }

    internal class CounterAuditSnapshot : CounterSnapshot, ICounterAuditProps
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

    #endregion

    public class TestCounter
    {

        [Fact]
        public void TestComposeActionOverride()
        {
            var Counter = Types.
                      Object<ICounterProps, ICounter>("Counter")
                     .Proxy(x => new CounterProxy(x))
                     .Snapshot(() => new CounterSnapshot())
                     .Mutable(o => o.Count, Types.Optional(Types.Int, 0))
                     .Action(o => o.Increment(), (o) => o.Count++);

            var CounterExt = Counter.Action(o => o.Increment(), (o) => o.Count += 10);

            var counter = CounterExt.Create();

            Assert.Equal(0, counter.Count);
            Assert.Equal(0, counter.GetSnapshot<ICounterProps>().Count);

            counter.Increment();

            Assert.Equal(10, counter.Count);
            Assert.Equal(10, counter.GetSnapshot<ICounterProps>().Count);
        }

        [Fact]
        public void TestGlobalReaction()
        {
            var Counter = Types.
                      Object<ICounterProps, ICounter>("Counter")
                     .Proxy(x => new CounterProxy(x))
                     .Snapshot(() => new CounterSnapshot())
                     .Mutable(o => o.Count, Types.Optional(Types.Int, 0))
                     .Action(o => o.Increment(), (o) => o.Count++);

            var counter = Counter.Create();

            counter.Unprotected();

            var effectCount = 0;

            var disposable = Reactions.Reaction(() =>
            {
                var _1 = counter.Count;
                var _2 = counter.Count;
            }, () =>
            {
                effectCount++;
            });

            counter.Increment();
            counter.Increment();

            disposable.Dispose();

            counter.Increment();
            counter.Increment();

            Assert.Equal(4, counter.Count);
            Assert.Equal(2, effectCount);
        }

        [Fact]
        public void TestComponseAddProps()
        {
            var Counter = Types.
                    Object<ICounterProps, ICounter>("Counter")
                   .Proxy(x => new CounterProxy(x))
                   .Snapshot(() => new CounterSnapshot())
                   .Mutable(o => o.Count, Types.Optional(Types.Int, 0))
                   .Action(o => o.Increment(), (o) => o.Count++);

            var CounterAudit = Types.Compose<ICounterAuditProps, ICounterAudit, ICounterProps, ICounter>("CounterAudit", Counter)
                .Proxy(x => new CounterAuditProxy(x))
                .Snapshot(() => new CounterAuditSnapshot())
                .Mutable(o => o.Called, Types.Optional(Types.Int, 0));

            var counter = CounterAudit.Create();

            Assert.Equal(0, counter.Count);
            Assert.Equal(0, counter.Called);

            counter.Unprotected();

            counter.Called = 20;

            Assert.Equal(20, counter.Called);

            Assert.Equal(0, counter.GetSnapshot<ICounterProps>().Count);
            Assert.Equal(20, counter.GetSnapshot<ICounterAuditProps>().Called);
        }
    }
}
