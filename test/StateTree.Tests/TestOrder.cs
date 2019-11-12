using Newtonsoft.Json;
using Skclusive.Mobx.Observable;
using System.Collections.Generic;
using Xunit;

namespace Skclusive.Mobx.StateTree.Tests
{
    #region IOrder

    public interface IOrder
    {
        double Vat { get; set; }

        double Price { get; set; }

        double PriceWithVat { get; }

        void UpdateVat(double vat);

        double IncrementPrice(double price);

        double DecrementPrice(double price);
    }

    internal class OrderProxy : ObservableProxy<IOrder, INode>, IOrder
    {
        public override IOrder Proxy => this;

        public OrderProxy(IObservableObject<IOrder, INode> target) : base(target)
        {
        }

        public double Vat
        {
            get => Read<double>(nameof(Vat));
            set => Write(nameof(Vat), value);
        }

        public double Price
        {
            get => Read<double>(nameof(Price));
            set => Write(nameof(Price), value);
        }

        public double PriceWithVat => Read<double>(nameof(PriceWithVat));

        public double IncrementPrice(double price)
        {
            return (Target as dynamic).IncrementPrice(price);
        }

        public double DecrementPrice(double price)
        {
            return (Target as dynamic).DecrementPrice(price);
        }

        public void UpdateVat(double vat)
        {
            (Target as dynamic).UpdateVat(vat);
        }
    }

    #endregion

    public class TestOrder
    {
        private static IObjectType<IOrder> Order = Types
                .Object<IOrder>("Order")
                .Proxy(x => new OrderProxy(x))
                .Mutable(o => o.Price, Types.Double)
                .Mutable(o => o.Vat, Types.Double)
                .View(o => o.PriceWithVat, Types.Double, (o) => o.Price * (1 + o.Vat))
                .Action<double>(o => o.UpdateVat(0), (o, vat) => o.Vat = vat)
                .Action<double, double>(o => o.IncrementPrice(0), (o, amount) => o.Price += amount)
                .Action<double, double>(o => o.DecrementPrice(0), (o, amount) =>
                {
                    var half = amount / 2;
                    o.Price -= half;
                    o.Price -= half;
                    return o.Price;
                });

        #region Order Tests

        [Fact]
        public void TestSetup()
        {
            var order = Order.Create(new Dictionary<string, object> { { "Vat", 1.0 } });

            Assert.NotNull(order);

            Assert.Equal(0, order.Price);
            Assert.Equal(1, order.Vat);
            Assert.Equal(0, order.PriceWithVat);
        }

        [Fact]
        public void TestPatchRecorder()
        {
            var order = Order.Create(new Dictionary<string, object> { { "Vat", 3.0 } }, new TestEnv());

            var target = Order.Create(new Dictionary<string, object> { { "Vat", 1.0 } }, new TestEnv());

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
            var order = Order.Create(new Dictionary<string, object> { { "Vat", 3.0 } }, new TestEnv());

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
            var order = Order.Create(new Dictionary<string, object> { { "Vat", 3.0 } }, new TestEnv());

            var target = Order.Create(new Dictionary<string, object> { { "Vat", 1.0 } }, new TestEnv());

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
