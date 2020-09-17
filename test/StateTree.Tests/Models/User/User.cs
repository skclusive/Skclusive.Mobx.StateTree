using Skclusive.Mobx.Observable;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Skclusive.Mobx.StateTree.Tests
{
    public interface IUserSnapshot
    {
        string Id { set; get; }

        string Name { set; get; }
    }

    public interface IUser : IUserSnapshot
    {
    }

    internal class UserSnapshot : IUserSnapshot
    {
        public string Id { set; get; }

        public string Name { set; get; }
    }

    internal class UserProxy : ObservableProxy<IUser, INode>, IUser
    {
        public override IUser Proxy => this;

        public UserProxy(IObservableObject<IUser, INode> target) : base(target)
        {
        }

        public string Id
        {
            get => Read<string>(nameof(Id));
            set => Write(nameof(Id), value);
        }

        public new string Name
        {
            get => Read<string>(nameof(Name));
            set => Write(nameof(Name), value);
        }
    }


    public partial class TestTypes
    {
        public readonly static IObjectType<IUserSnapshot, IUser> UserType = Types
                      .Object<IUserSnapshot, IUser>("IUser")
                      .Proxy(x => new UserProxy(x))
                      .Snapshot(() => new UserSnapshot())
                      .Mutable(o => o.Id, Types.Identifier())
                      .Mutable(o => o.Name, Types.String);

        public readonly static IObjectType<string, IUser> UserRefType = Types
              .Object<string, IUser>("IUser")
              .Proxy(x => new UserProxy(x))
              .Snapshot(() => "")
              .Mutable(o => o.Id, Types.Identifier())
              .Mutable(o => o.Name, Types.String);
    }
}
