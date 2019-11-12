using Skclusive.Mobx.Observable;
using System.Collections.Generic;
using Xunit;

namespace Skclusive.Mobx.StateTree.Tests
{
    #region IItem

    internal interface IITem
    {
        string Value { set; get; }
    }

    internal class ItemProxy : ObservableProxy<IITem, INode>, IITem
    {
        public override IITem Proxy => this;

        public ItemProxy(IObservableObject<IITem, INode> target) : base(target)
        {
        }

        public string Value
        {
            get => Read<string>(nameof(Value));
            set => Write(nameof(Value), value);
        }
    }

    internal interface IItemModel
    {
        IITem Item { set; get; }
    }

    internal class ItemModelProxy : ObservableProxy<IItemModel, INode>, IItemModel
    {
        public override IItemModel Proxy => this;

        public ItemModelProxy(IObservableObject<IItemModel, INode> target) : base(target)
        {
        }

        public IITem Item
        {
            get => Read<IITem>(nameof(Item));
            set => Write(nameof(Item), value);
        }
    }

    #endregion

    public class TestItemModel
    {
        [Fact]
        public void TestComplextMayBeType()
        {
            var Item = Types.Object<IITem>("Item")
                .Proxy(x => new ItemProxy(x))
                .Mutable(i => i.Value, Types.String);

            var Model = Types.Object<IItemModel>("ItemModel")
                .Proxy(x => new ItemModelProxy(x))
                .Mutable(i => i.Item, Types.Maybe(Item));

            var model = Model.Create();

            Assert.NotNull(model);
            Assert.Null(model.Item);

            model.ApplySnapshot(new Dictionary<string, object>
            {
                {
                    "Item",
                    new Dictionary<string, object>
                    {
                        {
                            "Value",
                            "Something"
                        }
                    }
                }
            });

            Assert.NotNull(model.Item);

            Assert.Equal("Something", model.Item.Value);

            model.ApplySnapshot(new Dictionary<string, object>
            {
                {
                    "Item",
                    null
                }
            });

            Assert.Null(model.Item);

            var snapshot = model.GetSnapshot<IDictionary<string, object>>();

            Assert.NotNull(snapshot);
            Assert.True(snapshot.ContainsKey("Item"));
            Assert.Null(snapshot["Item"]);
        }
    }
}
