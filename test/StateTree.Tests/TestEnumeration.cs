using Xunit;
using static Skclusive.Mobx.StateTree.Tests.TestTypes;

namespace Skclusive.Mobx.StateTree.Tests
{
    public class TestEnumeration
    {
        [Fact]
        public void TestCreate()
        {
            var filter = FilterType.Create(Filter.None);

            Assert.Equal(Filter.None, filter);
        }

        [Fact]
        public void TestCreateSnapshot()
        {
            var filter = FilterType.Create(Filter.Completed);

            Assert.Equal(Filter.Completed, filter);
        }
    }
}
