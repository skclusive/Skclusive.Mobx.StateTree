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

    public partial class TestTypes
    {
        public readonly static IObjectType<IOrder> OrderType = Types
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
    }
}
