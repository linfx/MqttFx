using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;
using System;
using System.Threading.Tasks;

namespace MqttFx.Utils
{
    class PendingUnSubscription
    {
        private readonly RetransmissionHandler<UnsubscribePacket> retransmissionHandler = new();

        public TaskCompletionSource<UnSubscribeResult> Future { get; set; } = new();

        public PendingUnSubscription(UnsubscribePacket packet)
        {
            retransmissionHandler.OriginalMessage = packet;
        }

        public void StartRetransmitTimer(IEventLoop eventLoop, Func<Packet, Task> send)
        {
            retransmissionHandler.SetHandle(originalMessage =>
            {
                send(new UnsubscribePacket(originalMessage.FixedHeader, (PacketIdVariableHeader)originalMessage.VariableHeader, (UnsubscribePayload)originalMessage.Payload));
            });
            retransmissionHandler.Start(eventLoop);
        }

        public void OnUnsubackReceived()
        {
            retransmissionHandler.Stop();
        }
    }
}
