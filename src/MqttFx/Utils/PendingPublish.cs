using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;
using System;
using System.Threading.Tasks;

namespace MqttFx.Utils;

class PendingPublish
{
    private readonly RetransmissionHandler<PublishPacket> publishRetransmissionHandler = new();
    private readonly RetransmissionHandler<PubRelPacket> pubRelRetransmissionHandler = new();

    public TaskCompletionSource<PublishResult> Future { get; set; } = new();

    public PendingPublish(PublishPacket packet)
    {
        publishRetransmissionHandler.OriginalMessage = packet;
    }
    public void SetPubRelMessage(PubRelPacket packet) => pubRelRetransmissionHandler.OriginalMessage = packet;

    public void StartPublishRetransmissionTimer(IEventLoop eventLoop, Func<Packet, Task> send)
    {
        publishRetransmissionHandler.SetHandle(originalMessage =>
        {
            var packet = originalMessage with { Dup = true };
            send(packet);
        });
        publishRetransmissionHandler.Start(eventLoop);
    }

    public void StartPubrelRetransmissionTimer(IEventLoop eventLoop, Func<Packet, Task> send)
    {
        pubRelRetransmissionHandler.SetHandle(originalMessage =>
        {
            send(new PubRelPacket(originalMessage.PacketId));
        });
        pubRelRetransmissionHandler.Start(eventLoop);
    }

    public void OnPubAckReceived()
    {
        //publishRetransmissionHandler.Stop();
    }

    public void OnPubCompReceived()
    {
        pubRelRetransmissionHandler.Stop();
    }
}
