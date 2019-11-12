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

    #region IColorBox

    public interface IColorBox : IColor, IBox
    {
    }

    internal class ColorBoxSnapshot : IColorBox
    {
        public string Color { set; get; }

        public int Width { set; get; }

        public int Height { set; get; }
    }

    internal class ColorBoxProxy : ObservableProxy<IColorBox, INode>, IColorBox
    {
        public override IColorBox Proxy => this;

        public ColorBoxProxy(IObservableObject<IColorBox, INode> target) : base(target)
        {
        }

        public string Color
        {
            get => Read<string>(nameof(Color));
            set => Write(nameof(Color), value);
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

    #region IColorShape

    public interface IColorShapeProps : IColor, IShapeProps
    {
    }

    public interface IColorShape : IColor, IShape
    {
    }

    internal class ColorShapeSnapshot : IColorShapeProps
    {
        public string Color { set; get; }

        public int Width { set; get; }

        public int Height { set; get; }
    }

    internal class ColorShapeProxy : ObservableProxy<IColorShape, INode>, IColorShape
    {
        public override IColorShape Proxy => this;

        public ColorShapeProxy(IObservableObject<IColorShape, INode> target) : base(target)
        {
        }

        public string Color
        {
            get => Read<string>(nameof(Color));
            set => Write(nameof(Color), value);
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

    public class TestCompose
    {

        private static IObjectType<IColor, IColor> Color = Types.
                      Object<IColor, IColor>("Color")
                     .Proxy(x => new ColorProxy(x))
                     .Snapshot(() => new ColorSnapshot())
                     .Mutable(o => o.Color, Types.String, "#FFFFFF");

        private static IObjectType<IBox, IBox> Box = Types.
              Object<IBox, IBox>("Box")
             .Proxy(x => new BoxProxy(x))
             .Snapshot(() => new BoxSnapshot())
             .Mutable(o => o.Width, Types.Int, 0)
             .Mutable(o => o.Height, Types.Int, 0);

        private static IObjectType<IShapeProps, IShape> Shape = Types.
                Object<IShapeProps, IShape>("Shape")
               .Proxy(x => new ShapeProxy(x))
               .Snapshot(() => new ShapeSnapshot())
               .Mutable(o => o.Width, Types.Int, 0)
               .Mutable(o => o.Height, Types.Int, 0)
               .View(o => o.Area, Types.Int, (o) => o.Width * o.Height)
               .Action<int>(o => o.SetWidth(0), (o, width) => o.Width = width)
               .Action<int>(o => o.SetHeight(0), (o, height) => o.Height = height);

        #region Computed Tests

        [Fact]
        public void TestComputedProperty()
        {
            var shape = Shape.Create();

            shape.SetWidth(3);
            shape.SetHeight(2);

            Assert.Equal(6, shape.Area);
        }

        #endregion

        #region Compose Tests

        //private static IObjectType<IColorBox, IColorBox> ColorBox = Types.
        //              Compose<IColorBox, IColorBox, IColor, IColor, IBox, IBox>("ColorBox", Color, Box)
        //             .Proxy(x => new ColorBoxProxy(x))
        //             .Snapshot(() => new ColorBoxSnapshot());

        [Fact]
        public void TestComposeFactories()
        {
            var ColorBox = Types.
                        Compose<IColorBox, IColorBox, IColor, IColor, IBox, IBox>("ColorBox", Color, Box)
                       .Proxy(x => new ColorBoxProxy(x))
                       .Snapshot(() => new ColorBoxSnapshot());

            var colorBox = ColorBox.Create();

            Assert.Equal(0, colorBox.Width);
            Assert.Equal(0, colorBox.Height);
            Assert.Equal("#FFFFFF", colorBox.Color);
        }

        [Fact]
        public void TestComposeComputedFactories()
        {
            var ColorShape = Types.
                        Compose<IColorShapeProps, IColorShape, IColor, IColor, IShapeProps, IShape>("ColorShape", Color, Shape)
                       .Proxy(x => new ColorShapeProxy(x))
                       .Snapshot(() => new ColorShapeSnapshot());

            var colorShape = ColorShape.Create(new ColorShapeSnapshot { Width = 100, Height = 200 });

            Assert.Equal(100, colorShape.Width);
            Assert.Equal(200, colorShape.Height);
            Assert.Equal("#FFFFFF", colorShape.Color);
            Assert.Equal(20000, colorShape.Area);

            colorShape.Unprotected();

            colorShape.Color = "#000000";
            colorShape.SetHeight(300);

            Assert.Equal(300, colorShape.Height);
            Assert.Equal("#000000", colorShape.Color);
            Assert.Equal(30000, colorShape.Area);
        }

        #endregion
    }
}
