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

    public partial class TestTypes
    {
        public readonly static IObjectType<IModelSnapshot, IModel> ModelType = Types
                       .Object<IModelSnapshot, IModel>("Model")
                       .Proxy(x => new ModelProxy(x))
                       .Snapshot(() => new ModelSnapshot())
                       .Mutable(o => o.To, Types.String, "world")
                       .Action<string>(o => o.SetTo(null), (o, to) => o.To = to);
    }
}
