using Skclusive.Mobx.Observable;
using Skclusive.Mobx.StateTree;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Skclusive.Mobx.StateTree.Tests
{
    #region IUserMapStore

    public interface IUserMapStoreSnapshot
    {
        string User { set; get; }

        IMap<string, IUserSnapshot> Users { set; get; }
    }

    public interface IUserMapStoreActions
    {
    }

    public interface IUserMapStore : IUserMapStoreActions
    {
        IUser User { set; get; }

        IObservableMap<string, INode, IUser> Users { set; get; }
    }

    internal class UserMapStoreSnapshot : IUserMapStoreSnapshot
    {
        public string User { set; get; }

        public IMap<string, IUserSnapshot> Users { set; get; }
    }

    internal class UserMapStoreProxy : ObservableProxy<IUserMapStore, INode>, IUserMapStore
    {
        public override IUserMapStore Proxy => this;

        public UserMapStoreProxy(IObservableObject<IUserMapStore, INode> target) : base(target)
        {
        }

        public IObservableMap<string, INode, IUser> Users
        {
            get => Read<IObservableMap<string, INode, IUser>>(nameof(Users));
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
        public readonly static IObjectType<IUserMapStoreSnapshot, IUserMapStore> UserMapStoreType = Types.
                        Object<IUserMapStoreSnapshot, IUserMapStore>("UserMapStore")
                       .Proxy(x => new UserMapStoreProxy(x))
                       .Snapshot(() => new UserMapStoreSnapshot())
                       .Mutable(o => o.User, Types.Reference<string, IUserSnapshot, IUser>(UserType))
                       .Mutable(o => o.Users, Types.Map(UserType));
    }
}
