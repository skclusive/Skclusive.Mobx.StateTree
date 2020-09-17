using Skclusive.Mobx.Observable;
using Skclusive.Mobx.StateTree;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Skclusive.Mobx.StateTree.Tests
{
    #region IUserListStore

    public interface IUserListStoreSnapshot
    {
        string User { set; get; }

        IUserSnapshot[] Users { set; get; }
    }

    public interface IUserListStoreActions
    {
    }

    public interface IUserListStore : IUserListStoreActions
    {
        IUser User { set; get; }

        IList<IUser> Users { set; get; }
    }

    internal class UserListStoreSnapshot : IUserListStoreSnapshot
    {
        public string User { set; get; }

        public IUserSnapshot[] Users { set; get; }
    }

    internal class UserListStoreProxy : ObservableProxy<IUserListStore, INode>, IUserListStore
    {
        public override IUserListStore Proxy => this;

        public UserListStoreProxy(IObservableObject<IUserListStore, INode> target) : base(target)
        {
        }

        public IList<IUser> Users
        {
            get => Read<IList<IUser>>(nameof(Users));
            set => Write(nameof(Users), value);
        }

        public IUser User
        {
            get => Read<IUser>(nameof(User));
            set => Write(nameof(User), value);
        }
    }

    #endregion

    public partial class TestTypes
    {
        public readonly static IObjectType<IUserListStoreSnapshot, IUserListStore> UserListStoreType = Types.
                        Object<IUserListStoreSnapshot, IUserListStore>("UserListStore")
                       .Proxy(x => new UserListStoreProxy(x))
                       .Snapshot(() => new UserListStoreSnapshot())
                       .Mutable(o => o.User, Types.Reference<string, IUserSnapshot, IUser>(UserType))
                       .Mutable(o => o.Users, Types.List(UserType));
    }
}
