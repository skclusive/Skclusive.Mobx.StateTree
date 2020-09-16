using Newtonsoft.Json;
using Skclusive.Mobx.Observable;
using System.Collections.Generic;
using Xunit;
using static Skclusive.Mobx.StateTree.Tests.TestTypes;

namespace Skclusive.Mobx.StateTree.Tests
{
    public class TestOrder
    {
        #region Order Tests

        [Fact]
        public void TestSetup()
        {
            var order = OrderType.Create(new Dictionary<string, object> { { "Vat", 1.0 } });

            Assert.NotNull(order);

            Assert.Equal(0, order.Price);
            Assert.Equal(1, order.Vat);
            Assert.Equal(0, order.PriceWithVat);
        }

        [Fact]
        public void TestPatchRecorder()
        {
            var order = OrderType.Create(new Dictionary<string, object> { { "Vat", 3.0 } }, new TestEnv());

            var target = OrderType.Create(new Dictionary<string, object> { { "Vat", 1.0 } }, new TestEnv());

            order.Unprotected();
            target.Unprotected();

            using (var recorder = new PatchRecorder<IOrder>(order))
            {
                order.Price = 10;
                order.Vat = 2;
                order.Price = 80;

                var incr = order.IncrementPrice(40);
                var decr = order.DecrementPrice(20);

                Assert.Equal(120, incr);
                Assert.Equal(100, decr);

                Assert.Equal(100, order.Price);
                Assert.Equal(2, order.Vat);
                Assert.Equal(300, order.PriceWithVat);

                Assert.Equal(0, target.Price);
                Assert.Equal(1, target.Vat);
                Assert.Equal(0, target.PriceWithVat);

                recorder.Replay(target);

                Assert.Equal(100, target.Price);
                Assert.Equal(2, target.Vat);
                Assert.Equal(300, target.PriceWithVat);

                recorder.Undo(target);

                Assert.Equal(0, target.Price);
                Assert.Equal(3, target.Vat);
                Assert.Equal(0, target.PriceWithVat);
            }
        }

        [Fact]
        public void TestSnapshot()
        {
            var order = OrderType.Create(new Dictionary<string, object> { { "Vat", 3.0 } }, new TestEnv());

            Assert.NotNull(order);

            order.Unprotected();

            var patches = new List<IJsonPatch>();

            order.OnPatch((patch, _patch) =>
            {
                patches.Add(patch);
            });

            Assert.Equal(3.0, order.Vat);

            var snapshots = new List<string>();
            order.OnSnapshot((snap) =>
            {
                var json = JsonConvert.SerializeObject(snap);

                snapshots.Add(json);
            });

            order.Price = 10;

            Assert.Equal(10, order.Price);
            Assert.Equal(40, order.PriceWithVat);

            order.Price = 100;
            order.Vat = 2;

            var price = order.Price;

            Assert.Equal(100, order.Price);
            Assert.Equal(2, order.Vat);

            Assert.Equal(300, order.PriceWithVat);

            Assert.Equal(3, snapshots.Count);

            order.ApplySnapshot(JsonConvert.DeserializeObject<IDictionary<string, object>>(snapshots[0]));

            Assert.Equal(10, order.Price);
            Assert.Equal(40, order.PriceWithVat);
        }

        [Fact]
        public void TestActionRecorder()
        {
            var order = OrderType.Create(new Dictionary<string, object> { { "Vat", 3.0 } }, new TestEnv());

            var target = OrderType.Create(new Dictionary<string, object> { { "Vat", 1.0 } }, new TestEnv());

            using (var recorder = new ActionRecorder<IOrder>(order))
            {
                order.IncrementPrice(10);
                order.UpdateVat(2);
                order.IncrementPrice(70);

                var incr = order.IncrementPrice(40);
                var decr = order.DecrementPrice(20);

                Assert.Equal(120, incr);
                Assert.Equal(100, decr);

                Assert.Equal(100, order.Price);
                Assert.Equal(2, order.Vat);
                Assert.Equal(300, order.PriceWithVat);

                Assert.Equal(0, target.Price);
                Assert.Equal(1, target.Vat);
                Assert.Equal(0, target.PriceWithVat);

                recorder.Replay(target);

                Assert.Equal(100, target.Price);
                Assert.Equal(2, target.Vat);
                Assert.Equal(300, target.PriceWithVat);
            }
        }

        #endregion
    }
}
