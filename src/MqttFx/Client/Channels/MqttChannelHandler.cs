using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;
using MqttFx.Formatter;
using MqttFx.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MqttFx.Client.Channels;

/// <summary>
/// 发送和接收数据处理器
/// </summary>
class MqttChannelHandler : SimpleChannelInboundHandler<Packet>
{
    private readonly MqttClient client;
    private readonly TaskCompletionSource<ConnectResult> connectFuture;

    public MqttChannelHandler(MqttClient client, TaskCompletionSource<ConnectResult> connectFuture)
    {
        this.client = client;
        this.connectFuture = connectFuture;
    }

    /// <summary>
    /// 通道激活时触发，当客户端connect成功后，服务端就会接收到这个事件，从而可以把客户端的Channel记录下来
    /// </summary>
    /// <param name="context"></param>
    public override void ChannelActive(IChannelHandlerContext context)
    {
        var packet = new ConnectPacket();
        var variableHeader = (ConnectVariableHeader)packet.VariableHeader;
        var payload = (ConnectPayload)packet.Payload;

        variableHeader.ConnectFlags.CleanSession = client.Options.CleanSession;
        variableHeader.KeepAlive = client.Options.KeepAlive;
        payload.ClientId = client.Options.ClientId;
        if (client.Options.Credentials != null)
        {
            variableHeader.ConnectFlags.UsernameFlag = true;
            payload.UserName = client.Options.Credentials.Username;
            payload.Password = client.Options.Credentials.Username;
        }
        if (!string.IsNullOrEmpty(client.Options.WillTopic))
        {
            variableHeader.ConnectFlags.WillFlag = true;
            variableHeader.ConnectFlags.WillQos = client.Options.WillQos;
            variableHeader.ConnectFlags.WillRetain = client.Options.WillRetain;
            payload.WillTopic = client.Options.WillTopic;
            payload.WillMessage = client.Options.WillPayload;
        }
        context.WriteAndFlushAsync(packet);
    }

    /// <summary>
    /// 当收到对方发来的数据后，就会触发，参数msg就是发来的信息，可以是基础类型，也可以是序列化的复杂对象
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="packet"></param>
    protected override void ChannelRead0(IChannelHandlerContext ctx, Packet packet)
    {
        switch (packet)
        {
            case ConnAckPacket connAckPacket:
                ProcessMessage(ctx.Channel, connAckPacket);
                break;
            case PublishPacket publishPacket:
                ProcessMessage(ctx.Channel, publishPacket);
                break;
            case PubRecPacket pubRecPacket:
                ProcessMessage(ctx.Channel, pubRecPacket);
                break;
            case PubRelPacket pubRelPacket:
                ProcessMessage(ctx.Channel, pubRelPacket);
                break;
            case PubCompPacket pubCompPacket:
                ProcessMessage(ctx.Channel, pubCompPacket);
                break;
            case PubAckPacket pubAckPacket:
                ProcessMessage(ctx.Channel, pubAckPacket);
                break;
            case SubAckPacket subAckPacket:
                ProcessMessage(ctx.Channel, subAckPacket);
                break;
            case UnsubAckPacket unsubAckPacket:
                ProcessMessage(ctx.Channel, unsubAckPacket);
                break;
        }
    }

    async void ProcessMessage(IChannel channel, ConnAckPacket packet)
    {
        var variableHeader = (ConnAckVariableHeader)packet.VariableHeader;
        switch (variableHeader.ConnectReturnCode)
        {
            case ConnectReturnCode.CONNECTION_ACCEPTED:
                connectFuture.TrySetResult(new ConnectResult(ConnectReturnCode.CONNECTION_ACCEPTED));
                await client.OnConnected(new ConnectResult(ConnectReturnCode.CONNECTION_ACCEPTED));
                break;

            case ConnectReturnCode.CONNECTION_REFUSED_BAD_USER_NAME_OR_PASSWORD:
            case ConnectReturnCode.CONNECTION_REFUSED_IDENTIFIER_REJECTED:
            case ConnectReturnCode.CONNECTION_REFUSED_SERVER_UNAVAILABLE:
            case ConnectReturnCode.CONNECTION_REFUSED_UNACCEPTABLE_PROTOCOL_VERSION:
                connectFuture.TrySetResult(new ConnectResult(variableHeader.ConnectReturnCode));
                await channel.CloseAsync();
                break;
        }
    }

    void ProcessMessage(IChannel channel, PublishPacket packet)
    {
        switch (packet.Qos)
        {
            case MqttQos.AtMostOnce:
                InvokeProcessForIncomingPublish(packet);
                break;

            case MqttQos.AtLeastOnce:
                InvokeProcessForIncomingPublish(packet);
                if (packet.PacketId > 0)
                    channel.WriteAndFlushAsync(new PubAckPacket(packet.PacketId));
                break;

            case MqttQos.ExactlyOnce:
                if (packet.PacketId > 0)
                    channel.WriteAndFlushAsync(new PubRecPacket(packet.PacketId));
                break;
        }
    }

    void ProcessMessage(IChannel channel, PubRecPacket packet)
    {
        if (client.PendingPublishs.TryGetValue(packet.PacketId, out PendingPublish pending))
        {
            pending.OnPubAckReceived();

            PubRelPacket pubRelPacket = new(packet.PacketId);
            channel.WriteAndFlushAsync(pubRelPacket);

            pending.SetPubRelMessage(pubRelPacket);
            pending.StartPubrelRetransmissionTimer(client.EventLoop.GetNext(), client.SendAsync);
        }
    }

    void ProcessMessage(IChannel channel, PubRelPacket packet)
    {
        channel.WriteAndFlushAsync(new PubCompPacket(packet.PacketId));
    }

    void ProcessMessage(IChannel channel, PubCompPacket packet)
    {
        if (client.PendingPublishs.TryRemove(packet.PacketId, out PendingPublish pending))
        {
            pending.Future.TrySetResult(new PublishResult(packet.PacketId));
            pending.OnPubCompReceived();
        }
    }

    void ProcessMessage(IChannel channel, PubAckPacket packet)
    {
        if (client.PendingPublishs.TryRemove(packet.PacketId, out PendingPublish pending))
        {
            pending.Future.TrySetResult(new PublishResult(packet.PacketId));
            pending.OnPubAckReceived();
        }
    }

    void ProcessMessage(IChannel channel, SubAckPacket packet)
    {
        if (client.PendingSubscriptions.TryRemove(packet.PacketId, out PendingSubscription pending))
        {
            pending.OnSubackReceived();

            var items = new List<SubscribeResultItem>();

            for (int i = 0; i < pending.SubscribePacket.SubscriptionRequests.Count; i++)
            {
                items.Add(new SubscribeResultItem
                {
                    TopicFilter = pending.SubscribePacket.SubscriptionRequests[i].TopicFilter,
                    ResultCode = packet.ReturnCodes[i]
                });
            }

            pending.Future.TrySetResult(new SubscribeResult(items));
        }
    }

    void ProcessMessage(IChannel channel, UnsubAckPacket packet)
    {
        if (client.PendingUnSubscriptions.TryRemove(packet.PacketId, out PendingUnSubscription pending))
        {
            pending.Future.TrySetResult(new UnSubscribeResult(packet.PacketId));
            pending.OnUnsubackReceived();
        }
    }

    void InvokeProcessForIncomingPublish(PublishPacket packet)
    {
        client.OnApplicationMessageReceived(packet.ToApplicationMessage());
    }
}
