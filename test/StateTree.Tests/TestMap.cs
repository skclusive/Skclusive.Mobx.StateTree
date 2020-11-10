using Skclusive.Core.Collection;
using Skclusive.Mobx.Observable;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Skclusive.Mobx.StateTree.Tests.TestTypes;

namespace Skclusive.Mobx.StateTree.Tests
{
    public class TestMap
    {
        private static IType<IMap<string, IModelSnapshot>, IObservableMap<string, INode, IModel>> ModelMapType = Types.Map(ModelType);

       [Fact]
        public void TestFactory()
        {
            var modelMap = ModelMapType.Create();

            Assert.NotNull(modelMap);
        }

        [Fact]
        public void TestSnapshot()
        {
            var modelMap = ModelMapType.Create
            (
                new Map<string, IModelSnapshot>
                {
                    { "hello", new ModelSnapshot { To = "world" } }
                }
            );

            Assert.NotNull(modelMap);
            Assert.True(modelMap.ContainsKey("hello"));
            Assert.Equal(1, modelMap.Keys.Count);

            Assert.True(modelMap["hello"] is IModelSnapshot);
            Assert.Equal("world", modelMap["hello"].To);

            var snapshot = modelMap.GetSnapshot<IMap<string, IModelSnapshot>>();

            Assert.NotNull(snapshot);
            Assert.True(snapshot.ContainsKey("hello"));
            Assert.Equal(1, snapshot.Keys.Count);

            Assert.True(snapshot["hello"] is IModelSnapshot);
            Assert.Equal("world", snapshot["hello"].To);
        }

        [Fact]
        public void TestEmitSnapshot()
        {
            var modelMap = ModelMapType.Create();

            modelMap.Unprotected();

            var snapshots = new List<IMap<string, IModelSnapshot>>();

            modelMap.OnSnapshot<IMap<string, IModelSnapshot>>(snapshot => snapshots.Add(snapshot));

            modelMap["hello"] = ModelType.Create();

            var snapshot = snapshots[0];

            Assert.NotNull(snapshot);
            Assert.True(snapshot.ContainsKey("hello"));
            Assert.Equal(1, snapshot.Keys.Count);

            Assert.True(snapshot["hello"] is IModelSnapshot);
            Assert.Equal("world", snapshot["hello"].To);
        }

        [Fact]
        public void TestAppySnapshot()
        {
            var modelMap = ModelMapType.Create();

            modelMap.Unprotected();

            modelMap.ApplySnapshot(new Map<string, IModelSnapshot>
            {
                { "hello", new ModelSnapshot { To = "world" } }
            });

            Assert.True(modelMap.ContainsKey("hello"));
            Assert.Equal(1, modelMap.Keys.Count);

            Assert.True(modelMap["hello"] is IModelSnapshot);
            Assert.Equal("world", modelMap["hello"].To);

            var snapshot = modelMap.GetSnapshot<IMap<string, IModelSnapshot>>();

            Assert.NotNull(snapshot);
            Assert.True(snapshot.ContainsKey("hello"));
            Assert.Equal(1, snapshot.Keys.Count);

            Assert.True(snapshot["hello"] is IModelSnapshot);
            Assert.Equal("world", snapshot["hello"].To);
        }

        [Fact]
        public void TestUpdatedSnapshot()
        {
            var modelMap = ModelMapType.Create();

            modelMap.Unprotected();

            modelMap["hello"] = ModelType.Create();

            Assert.True(modelMap.ContainsKey("hello"));
            Assert.Equal(1, modelMap.Keys.Count);

            Assert.True(modelMap["hello"] is IModelSnapshot);
            Assert.Equal("world", modelMap["hello"].To);

            var snapshot = modelMap.GetSnapshot<IMap<string, IModelSnapshot>>();

            Assert.NotNull(snapshot);
            Assert.True(snapshot.ContainsKey("hello"));
            Assert.Equal(1, snapshot.Keys.Count);

            Assert.True(snapshot["hello"] is IModelSnapshot);
            Assert.Equal("world", snapshot["hello"].To);
        }

        [Fact]
        public void TestAppyIsSameSnapshot()
        {
            var modelMap = ModelMapType.Create();

            modelMap.Unprotected();

            modelMap.ApplySnapshot(new Map<string, IModelSnapshot>
            {
                { "hello", new ModelSnapshot { To = "world" } }
            });

            Assert.True(modelMap.ContainsKey("hello"));
            Assert.Equal(1, modelMap.Keys.Count);

            Assert.True(modelMap["hello"] is IModelSnapshot);
            Assert.Equal("world", modelMap["hello"].To);

            var snapshot = modelMap.GetSnapshot<IMap<string, IModelSnapshot>>();

            Assert.NotNull(snapshot);
            Assert.True(snapshot.ContainsKey("hello"));
            Assert.Equal(1, snapshot.Keys.Count);

            Assert.True(snapshot["hello"] is IModelSnapshot);
            Assert.Equal("world", snapshot["hello"].To);

            modelMap.ApplySnapshot(new Map<string, IModelSnapshot>
            {
                { "hello", new ModelSnapshot { To = "world" } }
            });

            Assert.True(modelMap.ContainsKey("hello"));
            Assert.Equal(1, modelMap.Keys.Count);

            Assert.True(modelMap["hello"] is IModelSnapshot);
            Assert.Equal("world", modelMap["hello"].To);

            snapshot = modelMap.GetSnapshot<IMap<string, IModelSnapshot>>();

            Assert.NotNull(snapshot);
            Assert.True(snapshot.ContainsKey("hello"));
            Assert.Equal(1, snapshot.Keys.Count);

            Assert.True(snapshot["hello"] is IModelSnapshot);
            Assert.Equal("world", snapshot["hello"].To);

        }

        [Fact]
        public void TestEmitAddPatch()
        {
            var modelMap = ModelMapType.Create();

            modelMap.Unprotected();

            var patches = new List<IJsonPatch>();

            modelMap.OnPatch((patch, _) => patches.Add(patch));

            modelMap["hello"] = ModelType.Create(new ModelSnapshot { To = "universe" });

            Assert.Single(patches);

            Assert.NotNull(patches[0]);

            Assert.Equal(JsonPatchOperation.Add, patches[0].Operation);
            Assert.Equal("/hello", patches[0].Path);
            Assert.True(patches[0].Value is IModelSnapshot);
            Assert.Equal("universe", (patches[0].Value as IModelSnapshot).To);
        }

        [Fact]
        public void TestAppyAddPatch()
        {
            var modelMap = ModelMapType.Create();

            modelMap.Unprotected();

            modelMap.ApplyPatch(new JsonPatch
            {
                Operation = JsonPatchOperation.Add,

                Path = "hello",

                Value = new ModelSnapshot { To = "universe" }
            });

            Assert.True(modelMap.ContainsKey("hello"));
            Assert.Equal(1, modelMap.Keys.Count);

            Assert.True(modelMap["hello"] is IModelSnapshot);
            Assert.Equal("universe", modelMap["hello"].To);

            var snapshot = modelMap.GetSnapshot<IMap<string, IModelSnapshot>>();

            Assert.NotNull(snapshot);
            Assert.True(snapshot.ContainsKey("hello"));
            Assert.Equal(1, snapshot.Keys.Count);

            Assert.True(snapshot["hello"] is IModelSnapshot);
            Assert.Equal("universe", snapshot["hello"].To);
        }

        [Fact]
        public void TestEmitUpdatePatch()
        {
            var modelMap = ModelMapType.Create();

            modelMap.Unprotected();

            modelMap["hello"] = ModelType.Create();

            var patches = new List<IJsonPatch>();

            modelMap.OnPatch((patch, _) => patches.Add(patch));

            modelMap["hello"] = ModelType.Create(new ModelSnapshot { To = "universe" });

            Assert.Single(patches);

            Assert.NotNull(patches[0]);

            Assert.Equal(JsonPatchOperation.Replace, patches[0].Operation);
            Assert.Equal("/hello", patches[0].Path);
            Assert.True(patches[0].Value is IModelSnapshot);
            Assert.Equal("universe", (patches[0].Value as IModelSnapshot).To);
        }

        [Fact]
        public void TestAppyUpdatePatch()
        {
            var modelMap = ModelMapType.Create();

            modelMap.Unprotected();

            modelMap.ApplyPatch(new JsonPatch
            {
                Operation = JsonPatchOperation.Replace,

                Path = "hello",

                Value = new ModelSnapshot { To = "universe" }
            });

            Assert.True(modelMap.ContainsKey("hello"));
            Assert.Equal(1, modelMap.Keys.Count);

            Assert.True(modelMap["hello"] is IModelSnapshot);
            Assert.Equal("universe", modelMap["hello"].To);

            var snapshot = modelMap.GetSnapshot<IMap<string, IModelSnapshot>>();

            Assert.NotNull(snapshot);
            Assert.True(snapshot.ContainsKey("hello"));
            Assert.Equal(1, snapshot.Keys.Count);

            Assert.True(snapshot["hello"] is IModelSnapshot);
            Assert.Equal("universe", snapshot["hello"].To);
        }

        [Fact]
        public void TestEmitRemovePatch()
        {
            var modelMap = ModelMapType.Create();

            modelMap.Unprotected();

            modelMap["hello"] = ModelType.Create();

            var patches = new List<IJsonPatch>();

            modelMap.OnPatch((patch, _) => patches.Add(patch));

            modelMap.Remove("hello");

            Assert.Single(patches);

            Assert.NotNull(patches[0]);

            Assert.Equal(JsonPatchOperation.Remove, patches[0].Operation);
            Assert.Equal("/hello", patches[0].Path);
            Assert.Null(patches[0].Value);
        }

        [Fact]
        public void TestAppyRemovePatch()
        {
            var modelMap = ModelMapType.Create();

            modelMap.Unprotected();

            modelMap["hello"] = ModelType.Create();

            modelMap.ApplyPatch(new JsonPatch
            {
                Operation = JsonPatchOperation.Remove,

                Path = "hello"
            });

            Assert.False(modelMap.ContainsKey("hello"));
            Assert.Equal(0, modelMap.Keys.Count);

            var snapshot = modelMap.GetSnapshot<IMap<string, IModelSnapshot>>();

            Assert.NotNull(snapshot);
            Assert.False(snapshot.ContainsKey("hello"));
            Assert.Equal(0, snapshot.Keys.Count);
        }

        [Fact]
        public void TestAppyPatches()
        {
            var modelMap = ModelMapType.Create();

            modelMap.Unprotected();

            modelMap.ApplyPatch(new JsonPatch
            {
                Operation = JsonPatchOperation.Add,

                Path = "hello",

                Value = new ModelSnapshot { To = "mars" }
            }, new JsonPatch
            {
                Operation = JsonPatchOperation.Replace,

                Path = "hello",

                Value = new ModelSnapshot { To = "universe" }
            });

            Assert.True(modelMap.ContainsKey("hello"));
            Assert.Equal(1, modelMap.Keys.Count);

            Assert.True(modelMap["hello"] is IModelSnapshot);
            Assert.Equal("universe", modelMap["hello"].To);

            var snapshot = modelMap.GetSnapshot<IMap<string, IModelSnapshot>>();

            Assert.NotNull(snapshot);
            Assert.True(snapshot.ContainsKey("hello"));
            Assert.Equal(1, snapshot.Keys.Count);

            Assert.True(snapshot["hello"] is IModelSnapshot);
            Assert.Equal("universe", snapshot["hello"].To);
        }
    }
}
