using Skclusive.Mobx.Observable;
using Xunit;

namespace Skclusive.Mobx.StateTree.Tests
{
    #region IBox

    public interface IBox
    {
        int Width { set; get; }

        int Height { set; get; }
    }

    internal class BoxSnapshot : IBox
    {
        public int Width { set; get; }

        public int Height { set; get; }
    }

    internal class BoxProxy : ObservableProxy<IBox, INode>, IBox
    {
        public override IBox Proxy => this;

        public BoxProxy(IObservableObject<IBox, INode> target) : base(target)
        {
        }

        public int Width
        {
            get => Read<int>(nameof(Width));
            set => Write(nameof(Width), value);
        }

        public int Height
        {
            get => Read<int>(nameof(Height));
            set => Write(nameof(Height), value);
        }
    }

    #endregion

    public partial class TestTypes
    {
        public readonly static IObjectType<IBox, IBox> BoxType = Types.
              Object<IBox, IBox>("Box")
             .Proxy(x => new BoxProxy(x))
             .Snapshot(() => new BoxSnapshot())
             .Mutable(o => o.Width, Types.Int, 0)
             .Mutable(o => o.Height, Types.Int, 0);
    }
}
