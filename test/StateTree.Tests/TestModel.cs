using Skclusive.Mobx.Observable;
using System.Collections.Generic;
using Xunit;

namespace Skclusive.Mobx.StateTree.Tests
{
    #region IModel

    public interface IModelSnapshot
    {
        string To { set; get; }
    }

    public interface IModel : IModelSnapshot
    {
        void SetTo(string to);
    }

    internal class ModelSnapshot : IModelSnapshot
    {
        public string To { set; get; }
    }

    internal class ModelProxy : ObservableProxy<IModel, INode>, IModel
    {
        public override IModel Proxy => this;

        public ModelProxy(IObservableObject<IModel, INode> target) : base(target)
        {
        }

        public string To
        {
            get => Read<string>(nameof(To));
            set => Write(nameof(To), value);
        }

        public void SetTo(string to)
        {
            (Target as dynamic).SetTo(to);
        }
    }

    #endregion

    public class TestModel
    {
        private static IObjectType<IModelSnapshot, IModel> Model = Types
                       .Object<IModelSnapshot, IModel>("Model")
                       .Proxy(x => new ModelProxy(x))
                       .Snapshot(() => new ModelSnapshot())
                       .Mutable(o => o.To, Types.String, "world")
                       .Action<string>(o => o.SetTo(null), (o, to) => o.To = to);

        #region Factory Tests

        [Fact]
        public void TestFactoryCreation()
        {
            var instance = Model.Create();

            Assert.Equal("world", instance.To);

            var snapshot = instance.GetSnapshot<IModelSnapshot>();

            Assert.NotNull(snapshot);

            Assert.Equal("world", snapshot.To);
        }

        [Fact]
        public void TestFactorySnapshotCreation()
        {
            var instance = Model.Create();

            Assert.Equal("world", instance.To);

            var snapshot = instance.GetSnapshot<IModelSnapshot>();

            Assert.NotNull(snapshot);

            Assert.Equal(typeof(ModelSnapshot), snapshot.GetType());

            Assert.Equal("world", snapshot.To);
        }

        [Fact]
        public void TestRestoreSnapshotState()
        {
            var instance = Model.Create(new ModelSnapshot { To = "universe" });

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
            var model = Model.Create();

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
            var model = Model.Create();

            var snapshot = model.GetSnapshot<IModelSnapshot>();

            Assert.NotNull(snapshot);
            Assert.Equal("world", snapshot.To);
        }

        [Fact]
        public void TestApplySnapshots()
        {
            var model = Model.Create();

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
            var model = Model.Create();

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
            var model = Model.Create();

            model.Unprotected();

            model.ApplyPatch(new JsonPatch { Operation = JsonPatchOperation.Replace, Path = "/To", Value = "universe" });

            var snapshot = model.GetSnapshot<IModelSnapshot>();

            Assert.NotNull(snapshot);
            Assert.Equal("universe", snapshot.To);
        }

        [Fact]
        public void TestApplyPatches()
        {
            var model = Model.Create();

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
            var model = Model.Create();

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
            var model = Model.Create();

            model.SetTo("universe");

            var snapshot = model.GetSnapshot<IModelSnapshot>();

            Assert.NotNull(snapshot);
            Assert.Equal("universe", snapshot.To);
        }

        [Fact]
        public void TestEmitActionCall()
        {
            var model = Model.Create();

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
            var model = Model.Create();

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
