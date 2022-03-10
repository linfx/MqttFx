using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;
using System;
using System.Threading.Tasks;

namespace MqttFx.Utils
{
    class PendingSubscription
    {
        private readonly RetransmissionHandler<SubscribePacket> retransmissionHandler = new();

        public TaskCompletionSource<SubscribeResult> Future { get; set; } = new();

        public SubscribePacket SubscribePacket { get; set; }

        public PendingSubscription(SubscribePacket packet)
        {
            SubscribePacket = packet;
            retransmissionHandler.OriginalMessage = packet;
        }

        public void StartRetransmitTimer(IEventLoop eventLoop, Func<Packet, Task> send)
        {
            retransmissionHandler.SetHandle(originalMessage =>
            {
                send(new SubscribePacket(originalMessage.FixedHeader, (PacketIdVariableHeader)originalMessage.VariableHeader, (SubscribePayload)originalMessage.Payload));
            });
            retransmissionHandler.Start(eventLoop);
        }

        public void OnSubackReceived()
        {
            retransmissionHandler.Stop();
        }
    }
}
