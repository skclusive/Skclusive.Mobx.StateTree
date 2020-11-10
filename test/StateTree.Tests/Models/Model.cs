using Skclusive.Mobx.Observable;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace Skclusive.Mobx.StateTree.Tests
{
    #region IModel

    public interface IModelSnapshot
    {
        string To { set; get; }
    }

    public interface IModelVolatile
    {
        internal string Initial { set; get; }
    }

    public interface IModel : IModelSnapshot, IModelVolatile
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

        string IModelVolatile.Initial
        {
            get => Read<string>(nameof(IModelVolatile.Initial));
            set => Write(nameof(IModelVolatile.Initial), value);
        }

        public void SetTo(string to)
        {
            (Target as dynamic).SetTo(to);
        }
    }

    #endregion

    public partial class TestTypes
    {
        public readonly static IObjectType<IModelSnapshot, IModel> ModelType = Types
            .Object<IModelSnapshot, IModel>("Model")
            .Proxy(x => new ModelProxy(x))
            .Snapshot(() => new ModelSnapshot())
            .Volatile(o => o.Initial)
            .Mutable(o => o.To, Types.String, "world")
            .Hook(Hook.AfterCreate, o =>
            {
                o.Initial = o.To;
            })
            .Hook(Hook.AfterCreate, o =>
            {
                o.To = o.To.ToUpper();
            })
            .Hook(Hook.AfterCreate, o =>
            {
                o.To = o.To.ToLower();
            })
            .Hook(Hook.AfterCreate, o =>
            {
                o.To = o.Initial;
            })
            .Action<string>(o => o.SetTo(null), (o, to) => o.To = to);
    }
}
