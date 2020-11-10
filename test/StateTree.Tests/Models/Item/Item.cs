using Skclusive.Mobx.Observable;
using System.Collections.Generic;
using Xunit;

namespace Skclusive.Mobx.StateTree.Tests
{
    #region IItem

    public interface IItem
    {
        string Value { set; get; }
    }

    public class ItemProxy : ObservableProxy<IItem, INode>, IItem
    {
        public override IItem Proxy => this;

        public ItemProxy(IObservableObject<IItem, INode> target) : base(target)
        {
        }

        public string Value
        {
            get => Read<string>(nameof(Value));
            set => Write(nameof(Value), value);
        }
    }

    #endregion

    public partial class TestTypes
    {
        public readonly static IObjectType<IItem> ItemType = Types.Object<IItem>("Item")
                .Proxy(x => new ItemProxy(x))
                .Mutable(i => i.Value, Types.String);
    }
}
