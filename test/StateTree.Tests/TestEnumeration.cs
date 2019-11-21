using Xunit;

namespace Skclusive.Mobx.StateTree.Tests
{
    public enum Filter : int
    {
        None = 0,

		Active = 1,

		Completed = 2,

        All = 3
    }

    public class TestEnumeration
    {
        private readonly static IType<Filter, Filter> FilterType = Types.Enumeration("Filter", Filter.None, Filter.Active, Filter.Completed, Filter.All);

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
