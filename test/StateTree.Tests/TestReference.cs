using Skclusive.Mobx.Observable;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Skclusive.Mobx.StateTree.Tests.TestTypes;

namespace Skclusive.Mobx.StateTree.Tests
{
    public class TestReference
    {
        [Fact]
        public void TestMapStore()
        {
            var store = UserMapStoreType.Create(new UserMapStoreSnapshot
            {
                User = "17",

                Users = new Map<string, IUserSnapshot>
                {
                    { "17", new UserSnapshot { Id = "17", Name = "Naguvan" } },
                    { "18", new UserSnapshot { Id = "18", Name = "Skclusive" } }
                }
            });

            store.Unprotected();

            Assert.NotNull(store);

            Assert.Equal("Skclusive", store.Users["18"].Name);
            Assert.Equal("18", store.Users["18"].Id);

            Assert.Equal("Naguvan", store.Users["17"].Name);
            Assert.Equal("17", store.Users["17"].Id);

            Assert.Equal("Naguvan", store.User.Name);

            store.User = store.Users["18"];

            Assert.Equal("Skclusive", store.User.Name);

            store.Users["18"].Name = "Kalai";

            Assert.Equal("Kalai", store.User.Name);

            var snapshot = store.GetSnapshot<IUserMapStoreSnapshot>();

            Assert.Equal("18", snapshot.User);

            Assert.Equal(2, snapshot.Users.Count);

            Assert.Equal("18", snapshot.Users["18"].Id);
            Assert.Equal("Kalai", snapshot.Users["18"].Name);

            Assert.Equal("17", snapshot.Users["17"].Id);
            Assert.Equal("Naguvan", snapshot.Users["17"].Name);
        }

        [Fact]
        public void TestListStore()
        {
            var store = UserListStoreType.Create(new UserListStoreSnapshot
            {
                User = "17",

                Users = new IUserSnapshot[]
                {
                    new UserSnapshot { Id = "17", Name = "Naguvan" },
                    new UserSnapshot { Id = "18", Name = "Skclusive" }
                }
            });

            store.Unprotected();

            Assert.NotNull(store);

            Assert.Equal("Skclusive", store.Users[1].Name);
            Assert.Equal("18", store.Users[1].Id);

            Assert.Equal("Naguvan", store.Users[0].Name);
            Assert.Equal("17", store.Users[0].Id);

            Assert.Equal("Naguvan", store.User.Name);

            store.User = store.Users[1];

            Assert.Equal("Skclusive", store.User.Name);

            store.Users[1].Name = "Kalai";

            Assert.Equal("Kalai", store.User.Name);

            var snapshot = store.GetSnapshot<IUserListStoreSnapshot>();

            Assert.Equal("18", snapshot.User);

            Assert.Equal(2, snapshot.Users.Length);

            Assert.Equal("18", snapshot.Users[1].Id);
            Assert.Equal("Kalai", snapshot.Users[1].Name);

            Assert.Equal("17", snapshot.Users[0].Id);
            Assert.Equal("Naguvan", snapshot.Users[0].Name);
        }
    }
}
