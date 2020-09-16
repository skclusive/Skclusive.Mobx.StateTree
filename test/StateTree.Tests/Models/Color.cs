using Skclusive.Mobx.Observable;
using Xunit;

namespace Skclusive.Mobx.StateTree.Tests
{
    #region IColor

    public interface IColor
    {
        string Color { set; get; }
    }

    internal class ColorSnapshot : IColor
    {
        public string Color { set; get; }
    }

    internal class ColorProxy : ObservableProxy<IColor, INode>, IColor
    {
        public override IColor Proxy => this;

        public ColorProxy(IObservableObject<IColor, INode> target) : base(target)
        {
        }

        public string Color
        {
            get => Read<string>(nameof(Color));
            set => Write(nameof(Color), value);
        }
    }

    #endregion

    public partial class TestTypes
    {
        public readonly static IObjectType<IColor, IColor> ColorType = Types.
                      Object<IColor, IColor>("Color")
                     .Proxy(x => new ColorProxy(x))
                     .Snapshot(() => new ColorSnapshot())
                     .Mutable(o => o.Color, Types.String, "#FFFFFF");
    }
}
