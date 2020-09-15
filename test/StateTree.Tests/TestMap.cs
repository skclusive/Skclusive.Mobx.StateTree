using Skclusive.Mobx.Observable;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Skclusive.Mobx.StateTree.Tests
{
    public interface IMailSnapshot
    {
        string To { set; get; }
    }

    public interface IMail : IMailSnapshot
    {
    }

    internal class MailSnapshot : IMailSnapshot
    {
        public string To { set; get; }
    }

    internal class MailProxy : ObservableProxy<IMail, INode>, IMail
    {
        public override IMail Proxy => this;

        public MailProxy(IObservableObject<IMail, INode> target) : base(target)
        {
        }

        public string To
        {
            get => Read<string>(nameof(To));
            set => Write(nameof(To), value);
        }
    }


    public class TestMap
    {
        private static IObjectType<IMailSnapshot, IMail> MailType = Types
                      .Object<IMailSnapshot, IMail>("IMail")
                      .Proxy(x => new MailProxy(x))
                      .Snapshot(() => new MailSnapshot())
                      .Mutable(o => o.To, Types.String, "world");

        private static IType<IMap<string, IMailSnapshot>, IObservableMap<string, INode, IMail>> MailMapType = Types.Map(MailType);

       [Fact]
        public void TestFactory()
        {
            var mailMap = MailMapType.Create();

            Assert.NotNull(mailMap);
        }

        [Fact]
        public void TestSnapshot()
        {
            var mailMap = MailMapType.Create
            (
                new Map<string, IMailSnapshot>
                {
                    { "hello", new MailSnapshot { To = "world" } }
                }
            );

            Assert.NotNull(mailMap);
            Assert.True(mailMap.ContainsKey("hello"));
            Assert.Equal(1, mailMap.Keys.Count);

            Assert.True(mailMap["hello"] is IMailSnapshot);
            Assert.Equal("world", mailMap["hello"].To);

            var snapshot = mailMap.GetSnapshot<IMap<string, IMailSnapshot>>();

            Assert.NotNull(snapshot);
            Assert.True(snapshot.ContainsKey("hello"));
            Assert.Equal(1, snapshot.Keys.Count);

            Assert.True(snapshot["hello"] is IMailSnapshot);
            Assert.Equal("world", snapshot["hello"].To);
        }

        [Fact]
        public void TestEmitSnapshot()
        {
            var mailMap = MailMapType.Create();

            mailMap.Unprotected();

            var snapshots = new List<IMap<string, IMailSnapshot>>();

            mailMap.OnSnapshot<IMap<string, IMailSnapshot>>(snapshot => snapshots.Add(snapshot));

            mailMap["hello"] = MailType.Create();

            var snapshot = snapshots[0];

            Assert.NotNull(snapshot);
            Assert.True(snapshot.ContainsKey("hello"));
            Assert.Equal(1, snapshot.Keys.Count);

            Assert.True(snapshot["hello"] is IMailSnapshot);
            Assert.Equal("world", snapshot["hello"].To);
        }

        [Fact]
        public void TestAppySnapshot()
        {
            var mailMap = MailMapType.Create();

            mailMap.Unprotected();

            mailMap.ApplySnapshot(new Map<string, IMailSnapshot>
            {
                { "hello", new MailSnapshot { To = "world" } }
            });

            Assert.True(mailMap.ContainsKey("hello"));
            Assert.Equal(1, mailMap.Keys.Count);

            Assert.True(mailMap["hello"] is IMailSnapshot);
            Assert.Equal("world", mailMap["hello"].To);

            var snapshot = mailMap.GetSnapshot<IMap<string, IMailSnapshot>>();

            Assert.NotNull(snapshot);
            Assert.True(snapshot.ContainsKey("hello"));
            Assert.Equal(1, snapshot.Keys.Count);

            Assert.True(snapshot["hello"] is IMailSnapshot);
            Assert.Equal("world", snapshot["hello"].To);
        }

        [Fact]
        public void TestUpdatedSnapshot()
        {
            var mailMap = MailMapType.Create();

            mailMap.Unprotected();

            mailMap["hello"] = MailType.Create();

            Assert.True(mailMap.ContainsKey("hello"));
            Assert.Equal(1, mailMap.Keys.Count);

            Assert.True(mailMap["hello"] is IMailSnapshot);
            Assert.Equal("world", mailMap["hello"].To);

            var snapshot = mailMap.GetSnapshot<IMap<string, IMailSnapshot>>();

            Assert.NotNull(snapshot);
            Assert.True(snapshot.ContainsKey("hello"));
            Assert.Equal(1, snapshot.Keys.Count);

            Assert.True(snapshot["hello"] is IMailSnapshot);
            Assert.Equal("world", snapshot["hello"].To);
        }

        [Fact]
        public void TestAppyIsSameSnapshot()
        {
            var mailMap = MailMapType.Create();

            mailMap.Unprotected();

            mailMap.ApplySnapshot(new Map<string, IMailSnapshot>
            {
                { "hello", new MailSnapshot { To = "world" } }
            });

            Assert.True(mailMap.ContainsKey("hello"));
            Assert.Equal(1, mailMap.Keys.Count);

            Assert.True(mailMap["hello"] is IMailSnapshot);
            Assert.Equal("world", mailMap["hello"].To);

            var snapshot = mailMap.GetSnapshot<IMap<string, IMailSnapshot>>();

            Assert.NotNull(snapshot);
            Assert.True(snapshot.ContainsKey("hello"));
            Assert.Equal(1, snapshot.Keys.Count);

            Assert.True(snapshot["hello"] is IMailSnapshot);
            Assert.Equal("world", snapshot["hello"].To);

            mailMap.ApplySnapshot(new Map<string, IMailSnapshot>
            {
                { "hello", new MailSnapshot { To = "world" } }
            });

            Assert.True(mailMap.ContainsKey("hello"));
            Assert.Equal(1, mailMap.Keys.Count);

            Assert.True(mailMap["hello"] is IMailSnapshot);
            Assert.Equal("world", mailMap["hello"].To);

            snapshot = mailMap.GetSnapshot<IMap<string, IMailSnapshot>>();

            Assert.NotNull(snapshot);
            Assert.True(snapshot.ContainsKey("hello"));
            Assert.Equal(1, snapshot.Keys.Count);

            Assert.True(snapshot["hello"] is IMailSnapshot);
            Assert.Equal("world", snapshot["hello"].To);

        }
    }
}
