using Skclusive.Mobx.Observable;
using System.Collections.Generic;
using Xunit;
using static Skclusive.Mobx.StateTree.Tests.TestTypes;

namespace Skclusive.Mobx.StateTree.Tests
{
    public class TestItemModel
    {
        [Fact]
        public void TestComplextMayBeType()
        {
            var model = ItemModelType.Create();

            Assert.NotNull(model);
            Assert.Null(model.Item);

            model.ApplySnapshot(new Dictionary<string, object>
            {
                {
                    "Item",
                    new Dictionary<string, object>
                    {
                        {
                            "Value",
                            "Something"
                        }
                    }
                }
            });

            Assert.NotNull(model.Item);

            Assert.Equal("Something", model.Item.Value);

            model.ApplySnapshot(new Dictionary<string, object>
            {
                {
                    "Item",
                    null
                }
            });

            Assert.Null(model.Item);

            var snapshot = model.GetSnapshot<IDictionary<string, object>>();

            Assert.NotNull(snapshot);
            Assert.True(snapshot.ContainsKey("Item"));
            Assert.Null(snapshot["Item"]);
        }
    }
}
