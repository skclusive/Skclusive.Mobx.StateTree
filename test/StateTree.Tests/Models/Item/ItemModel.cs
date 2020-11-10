using Skclusive.Mobx.Observable;
using System.Collections.Generic;
using Xunit;

namespace Skclusive.Mobx.StateTree.Tests
{
    #region IItemModel

    public interface IItemModel
    {
        IItem Item { set; get; }
    }

    public class ItemModelProxy : ObservableProxy<IItemModel, INode>, IItemModel
    {
        public override IItemModel Proxy => this;

        public ItemModelProxy(IObservableObject<IItemModel, INode> target) : base(target)
        {
        }

        public IItem Item
        {
            get => Read<IItem>(nameof(Item));
            set => Write(nameof(Item), value);
        }
    }

    #endregion

    public partial class TestTypes
    {
         public readonly static IObjectType<IItemModel> ItemModelType = Types.Object<IItemModel>("ItemModel")
                .Proxy(x => new ItemModelProxy(x))
                .Mutable(i => i.Item, Types.Maybe(ItemType));
    }
}
