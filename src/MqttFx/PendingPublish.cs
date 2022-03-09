using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;
using MqttFx.Client.Channels;
using System;
using System.Threading.Tasks;

namespace MqttFx
{
    class PendingPublish
    {
        private readonly RetransmissionHandler<PublishPacket> publishRetransmissionHandler = new();
        private readonly RetransmissionHandler<PubRelPacket> pubRelRetransmissionHandler = new();

        public TaskCompletionSource<PublishResult> Future { get; set; } = new();

        public PendingPublish(PublishPacket message)
        {
            publishRetransmissionHandler.OriginalMessage = message;
        }

        public void StartPublishRetransmissionTimer(IEventLoop eventLoop, Action<Packet> sendPacket)
        {
            publishRetransmissionHandler.SetHandle(originalMessage =>
            {
                var packet = new PublishPacket(originalMessage.FixedHeader, (PublishVariableHeader)originalMessage.VariableHeader, (PublishPayload)originalMessage.Payload)
                {
                    Dup = true
                };
                sendPacket(packet);
            });
            publishRetransmissionHandler.Start(eventLoop);
        }

        public void OnPubAckReceived()
        {
            publishRetransmissionHandler.Stop();
        }

        public void SetPubRelMessage(PubRelPacket packet) => pubRelRetransmissionHandler.OriginalMessage = packet;

        public void StartPubrelRetransmissionTimer(IEventLoop eventLoop, Action<Packet> sendPacket)
        {
            pubRelRetransmissionHandler.SetHandle(originalMessage =>
            {
                var packet = new PubRelPacket(originalMessage.PacketId);
                sendPacket(packet);
            });
            pubRelRetransmissionHandler.Start(eventLoop);
        }

        public void OnPubCompReceived()
        {
            pubRelRetransmissionHandler.Stop();
        }
    }
}
