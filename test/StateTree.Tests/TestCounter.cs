using Skclusive.Mobx.Observable;
using Xunit;
using static Skclusive.Mobx.StateTree.Tests.TestTypes;

namespace Skclusive.Mobx.StateTree.Tests
{
    public class TestCounter
    {

        [Fact]
        public void TestComposeActionOverride()
        {
            var CounterExt = CounterType.Action(o => o.Increment(), (o) => o.Count += 10);

            var counter = CounterExt.Create();

            Assert.Equal(0, counter.Count);
            Assert.Equal(0, counter.GetSnapshot<ICounterSnapshot>().Count);

            counter.Increment();

            Assert.Equal(10, counter.Count);
            Assert.Equal(10, counter.GetSnapshot<ICounterSnapshot>().Count);
        }

        [Fact]
        public void TestGlobalReaction()
        {
            var counter = CounterType.Create();

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
            var CounterAudit = Types.Compose<ICounterAuditSnapshot, ICounterAudit, ICounterSnapshot, ICounter>("CounterAudit", CounterType)
                .Proxy(x => new CounterAuditProxy(x))
                .Snapshot(() => new CounterAuditSnapshot())
                .Mutable(o => o.Called, Types.Optional(Types.Int, 0));

            var counter = CounterAudit.Create();

            Assert.Equal(0, counter.Count);
            Assert.Equal(0, counter.Called);

            counter.Unprotected();

            counter.Called = 20;

            Assert.Equal(20, counter.Called);

            Assert.Equal(0, counter.GetSnapshot<ICounterSnapshot>().Count);
            Assert.Equal(20, counter.GetSnapshot<ICounterAuditSnapshot>().Called);
        }
    }
}
