using Skclusive.Mobx.Observable;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Skclusive.Mobx.StateTree.Tests
{
    public interface IBranchSnapshot
    {
        string Name { set; get; }
    }

    public interface IBranchActions
    {
        void EditName(string name);
    }

    public interface IBranch : IBranchSnapshot, IBranchActions
    {
    }

    internal class BranchSnapshot : IBranchSnapshot
    {
        public string Name { set; get; }
    }

    internal class BranchProxy : ObservableProxy<IBranch, INode>, IBranch
    {
        public override IBranch Proxy => this;

        public BranchProxy(IObservableObject<IBranch, INode> target) : base(target)
        {
        }

        public new string Name
        {
            get => Read<string>(nameof(Name));
            set => Write(nameof(Name), value);
        }

        public void EditName(string name)
        {
            (Target as dynamic).EditName(name);
        }
    }


    public partial class TestTypes
    {
        public readonly static IObjectType<IBranchSnapshot, IBranch> BranchType = Types
                      .Object<IBranchSnapshot, IBranch>("IBranch")
                      .Proxy(x => new BranchProxy(x))
                      .Snapshot(() => new BranchSnapshot())
                      .Mutable(o => o.Name, Types.String)
                      .Action<string>(o => o.EditName(default), (o, name) => o.Name = name);
    }
}
