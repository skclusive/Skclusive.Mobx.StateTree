using Skclusive.Mobx.Observable;
using System.Collections.Generic;
using Xunit;
using static Skclusive.Mobx.StateTree.Tests.TestTypes;

namespace Skclusive.Mobx.StateTree.Tests
{
    public class TestModel
    {
        #region Factory Tests

        [Fact]
        public void TestFactoryCreation()
        {
            var instance = ModelType.Create();

            Assert.Equal("world", instance.To);

            var snapshot = instance.GetSnapshot<IModelSnapshot>();

            Assert.NotNull(snapshot);

            Assert.Equal("world", snapshot.To);
        }

        [Fact]
        public void TestFactorySnapshotCreation()
        {
            var instance = ModelType.Create();

            Assert.Equal("world", instance.To);

            var snapshot = instance.GetSnapshot<IModelSnapshot>();

            Assert.NotNull(snapshot);

            Assert.Equal(typeof(ModelSnapshot), snapshot.GetType());

            Assert.Equal("world", snapshot.To);
        }

        [Fact]
        public void TestRestoreSnapshotState()
        {
            var instance = ModelType.Create(new ModelSnapshot { To = "universe" });

            Assert.Equal("universe", instance.To);

            var snapshot = instance.GetSnapshot<IModelSnapshot>();

            Assert.NotNull(snapshot);

            Assert.Equal(typeof(ModelSnapshot), snapshot.GetType());

            Assert.Equal("universe", snapshot.To);
        }

        #endregion

        #region Snapshot Tests

        [Fact]
        public void TestEmitSnapshots()
        {
            var model = ModelType.Create();

            model.Unprotected();

            var snapshots = new List<IModelSnapshot>();

            model.OnSnapshot<IModelSnapshot>(snapshot => snapshots.Add(snapshot));

            model.To = "universe";

            Assert.Single(snapshots);
            Assert.Equal("universe", snapshots[0].To);
        }

        [Fact]
        public void TestDefaultSnapshot()
        {
            var model = ModelType.Create();

            var snapshot = model.GetSnapshot<IModelSnapshot>();

            Assert.NotNull(snapshot);
            Assert.Equal("world", snapshot.To);
        }

        [Fact]
        public void TestApplySnapshots()
        {
            var model = ModelType.Create();

            // model.Unprotected();

            model.ApplySnapshot<IModelSnapshot>(new ModelSnapshot { To = "universe" });

            var snapshot = model.GetSnapshot<IModelSnapshot>();

            Assert.NotNull(snapshot);
            Assert.Equal("universe", snapshot.To);
        }

        #endregion

        #region Patch Tests

        [Fact]
        public void TestEmitPatches()
        {
            var model = ModelType.Create();

            model.Unprotected();

            var patches = new List<IJsonPatch>();

            model.OnPatch((patch, _) => patches.Add(patch));

            model.To = "universe";

            Assert.Single(patches);

            Assert.Equal(JsonPatchOperation.Replace, patches[0].Operation);
            Assert.Equal("/To", patches[0].Path);
            Assert.Equal("universe", patches[0].Value);
        }

        [Fact]
        public void TestApplyPatche()
        {
            var model = ModelType.Create();

            model.Unprotected();

            model.ApplyPatch(new JsonPatch { Operation = JsonPatchOperation.Replace, Path = "/To", Value = "universe" });

            var snapshot = model.GetSnapshot<IModelSnapshot>();

            Assert.NotNull(snapshot);
            Assert.Equal("universe", snapshot.To);
        }

        [Fact]
        public void TestApplyPatches()
        {
            var model = ModelType.Create();

            model.Unprotected();

            model.ApplyPatch
            (
                new JsonPatch { Operation = JsonPatchOperation.Replace, Path = "/To", Value = "mars" },
                new JsonPatch { Operation = JsonPatchOperation.Replace, Path = "/To", Value = "universe" }
            );

            var snapshot = model.GetSnapshot<IModelSnapshot>();

            Assert.NotNull(snapshot);
            Assert.Equal("universe", snapshot.To);
        }

        [Fact]
        public void TestDisposePatchListening()
        {
            var model = ModelType.Create();

            model.Unprotected();

            var patches = new List<IJsonPatch>();

            var disposable = model.OnPatch((patch, _) => patches.Add(patch));

            model.To = "universe";

            disposable.Dispose();

            model.To = "mars";

            Assert.Single(patches);

            Assert.Equal(JsonPatchOperation.Replace, patches[0].Operation);
            Assert.Equal("/To", patches[0].Path);
            Assert.Equal("universe", patches[0].Value);

            var snapshot = model.GetSnapshot<IModelSnapshot>();

            Assert.NotNull(snapshot);
            Assert.Equal("mars", snapshot.To);
        }

        #endregion

        #region Action Tests

        [Fact]
        public void TestActionIsCalled()
        {
            var model = ModelType.Create();

            model.SetTo("universe");

            var snapshot = model.GetSnapshot<IModelSnapshot>();

            Assert.NotNull(snapshot);
            Assert.Equal("universe", snapshot.To);
        }

        [Fact]
        public void TestEmitActionCall()
        {
            var model = ModelType.Create();

            var calls = new List<ISerializedActionCall>();

            model.OnAction(call => calls.Add(call));

            model.SetTo("universe");

            Assert.Single(calls);
            Assert.Equal("SetTo", calls[0].Name);
            Assert.Equal("", calls[0].Path);
            Assert.Equal("universe", calls[0].Arguments[0]);
        }

        [Fact]
        public void TestApplyActionCalls()
        {
            var model = ModelType.Create();

            var calls = new ISerializedActionCall[]
            {
                new SerializedActionCall
                {
                    Name = "SetTo",

                    Path = "",

                    Arguments = new object[] { "mars" }
                },

                new SerializedActionCall
                {
                    Name = "SetTo",

                    Path = "",

                    Arguments = new object[] { "universe" }
                }
            };

            model.ApplyAction(calls);

            var snapshot = model.GetSnapshot<IModelSnapshot>();

            Assert.NotNull(snapshot);
            Assert.Equal("universe", snapshot.To);
        }

        #endregion
    }
}
