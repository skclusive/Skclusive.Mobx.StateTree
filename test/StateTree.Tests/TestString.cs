using Skclusive.Mobx.Observable;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Skclusive.Mobx.StateTree.Tests
{
    internal class TestEnv : IEnvironment
    {
    }

    public class TestString
    {

        [Fact]
        public void TestCreate()
        {
            var hello = Types.String.Create("hello", new TestEnv());

            Assert.NotNull(hello);
            Assert.Equal("hello", hello);
        }

        [Fact]
        public void TestOptional()
        {
            var OptionalString = Types.Optional(Types.String, "sk");

            var optional = OptionalString.Create(null, new TestEnv());

            Assert.NotNull(optional);
            Assert.Equal("sk", optional);
        }

        [Fact]
        public void TestList()
        {
            var StringList = Types.List(Types.String);

            var values = StringList.Create(new string[] { "one", "two" }, new TestEnv());

            Assert.NotNull(values);
            Assert.Equal(2, values.Length);
            Assert.Equal("one", values[0]);
            Assert.Equal("two", values[1]);

            var autos = new List<string>();

            Reactions.Autorun((r) =>
            {
                autos.AddRange(values.ToList());
            });

            var patches = new List<IJsonPatch>();

            values.OnPatch((patch, _patch) =>
            {
                patches.Add(patch);
            });

            values.Unprotected();

            values.Add("three");

            Assert.Equal(5, autos.Count);
            Assert.Single(patches);
        }
    }
}
