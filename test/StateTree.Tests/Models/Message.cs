using Skclusive.Mobx.Observable;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Skclusive.Mobx.StateTree.Tests
{
    public interface IMessageSnapshot
    {
        string Id { set; get; }

        string Title { set; get; }
    }

    public interface IMessage : IMessageSnapshot
    {
    }

    internal class MessageSnapshot : IMessageSnapshot
    {
        public string Id { set; get; }

        public string Title { set; get; }
    }

    internal class MessageProxy : ObservableProxy<IMessage, INode>, IMessage
    {
        public override IMessage Proxy => this;

        public MessageProxy(IObservableObject<IMessage, INode> target) : base(target)
        {
        }

        public string Id
        {
            get => Read<string>(nameof(Id));
            set => Write(nameof(Id), value);
        }

        public string Title
        {
            get => Read<string>(nameof(Title));
            set => Write(nameof(Title), value);
        }
    }


    public partial class TestTypes
    {
        public readonly static IObjectType<IMessageSnapshot, IMessage> MessageType = Types
                      .Object<IMessageSnapshot, IMessage>("IMessage")
                      .Proxy(x => new MessageProxy(x))
                      .Snapshot(() => new MessageSnapshot())
                      .Mutable(o => o.Id, Types.Identifier)
                      .Mutable(o => o.Title, Types.String);
    }
}
