using Xunit;

namespace Skclusive.Mobx.StateTree.Tests
{
    public class TestList
    {
        [Fact]
        public void TestSnapshot()
        {
            var list = Types.List(Types.Int).Create(new int[] { 1 });

            list.Unprotected();

            list.Insert(0, 2);

            var snapshots = list.GetSnapshot<int[]>();

            Assert.Equal(2, snapshots.Length);
        }
    }
}
