using Skclusive.Mobx.Observable;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Skclusive.Mobx.StateTree.Tests.TestTypes;

namespace Skclusive.Mobx.StateTree.Tests
{
    public class TestIdentifier
    {
       [Fact]
        public void TestFactory()
        {
            foreach(var id in new []{ "coffee", "cof$fee", "cof|fee", "cof/fee" })
            {
                var message = MessageType.Create(new MessageSnapshot
                {
                    Id = id,

                    Title = "Get coffee"
                });

                Assert.NotNull(message);
            }
        }
    }
}
