using Skclusive.Mobx.Observable;
using Xunit;
using static Skclusive.Mobx.StateTree.Tests.TestTypes;

namespace Skclusive.Mobx.StateTree.Tests
{
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
        #region Computed Tests

        [Fact]
        public void TestComputedProperty()
        {
            var shape = ShapeType.Create();

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
            var ColorBoxType = Types.
                        Compose<IColorBox, IColorBox, IColor, IColor, IBox, IBox>("ColorBox", ColorType, BoxType)
                       .Proxy(x => new ColorBoxProxy(x))
                       .Snapshot(() => new ColorBoxSnapshot());

            var colorBox = ColorBoxType.Create();

            Assert.Equal(0, colorBox.Width);
            Assert.Equal(0, colorBox.Height);
            Assert.Equal("#FFFFFF", colorBox.Color);
        }

        [Fact]
        public void TestComposeComputedFactories()
        {
            var ColorShapeType = Types.
                        Compose<IColorShapeProps, IColorShape, IColor, IColor, IShapeProps, IShape>("ColorShape", ColorType, ShapeType)
                       .Proxy(x => new ColorShapeProxy(x))
                       .Snapshot(() => new ColorShapeSnapshot());

            var colorShape = ColorShapeType.Create(new ColorShapeSnapshot { Width = 100, Height = 200 });

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
