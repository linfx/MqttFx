using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;
using MqttFx.Client.Channels;
using System;
using System.Threading.Tasks;

namespace MqttFx
{
    class PendingPublish
    {
        private RetransmissionHandler<PublishPacket> publishRetransmissionHandler;
        private RetransmissionHandler<Packet> pubrelRetransmissionHandler;

        public TaskCompletionSource<PublishResult> Future { get; set; } = new();

        public PendingPublish(PublishPacket message)
        {
            publishRetransmissionHandler = new RetransmissionHandler<PublishPacket>(message);
            pubrelRetransmissionHandler = new RetransmissionHandler<Packet>(message);
        }

        public void StartPublishRetransmissionTimer(IEventLoop eventLoop, Action<Packet> sendPacket)
        {
            publishRetransmissionHandler.SetHandle(originalMessage =>
            {
                var packet = new PublishPacket(originalMessage.FixedHeader, (PublishVariableHeader)originalMessage.VariableHeader, (PublishPayload)originalMessage.Payload);
                packet.SetDup(true);
                sendPacket(packet);
            });
            publishRetransmissionHandler.Start(eventLoop);
        }

        public void OnPubackReceived()
        {
            publishRetransmissionHandler.Stop();
        }
    }
}
