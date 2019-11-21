using Xunit;

namespace Skclusive.Mobx.StateTree.Tests
{
    public enum Value : int
    {
        None = 0
    }

    public class TestLiteral
    {
        [Fact]
        public void TestCreate()
        {
            ISimpleType<Value> ValueType = Types.Literal(Value.None);

            var value = ValueType.Create();

            Assert.Equal(Value.None, value);
        }
    }
}
