using Skclusive.Mobx.Observable;
using Xunit;

namespace Skclusive.Mobx.StateTree.Tests
{
    #region IShape

    public interface IShapeProps
    {
        int Width { set; get; }

        int Height { set; get; }
    }

    public interface IShapeActions
    {
        void SetWidth(int width);

        void SetHeight(int height);
    }

    public interface IShape : IShapeProps, IShapeActions
    {
        int Area { get; }
    }

    internal class ShapeSnapshot : IShapeProps
    {
        public int Width { set; get; }

        public int Height { set; get; }
    }

    internal class ShapeProxy : ObservableProxy<IShape, INode>, IShape
    {
        public override IShape Proxy => this;

        public ShapeProxy(IObservableObject<IShape, INode> target) : base(target)
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

        public int Area
        {
            get => Read<int>(nameof(Area));
        }

        public void SetWidth(int width)
        {
            (Target as dynamic).SetWidth(width);
        }

        public void SetHeight(int height)
        {
            (Target as dynamic).SetHeight(height);
        }
    }

    #endregion

    public partial class TestTypes
    {
        public readonly static IObjectType<IShapeProps, IShape> ShapeType = Types.
                Object<IShapeProps, IShape>("Shape")
               .Proxy(x => new ShapeProxy(x))
               .Snapshot(() => new ShapeSnapshot())
               .Mutable(o => o.Width, Types.Int, 0)
               .Mutable(o => o.Height, Types.Int, 0)
               .View(o => o.Area, Types.Int, (o) => o.Width * o.Height)
               .Action<int>(o => o.SetWidth(0), (o, width) => o.Width = width)
               .Action<int>(o => o.SetHeight(0), (o, height) => o.Height = height);
    }
}
