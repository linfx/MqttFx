using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;
using System;
using System.Threading.Tasks;

namespace MqttFx.Utils;

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
        retransmissionHandler.Handler = originalMessage => send(originalMessage with { });
        retransmissionHandler.Start(eventLoop);
    }

    public void OnUnsubackReceived()
    {
        retransmissionHandler.Stop();
    }
}
