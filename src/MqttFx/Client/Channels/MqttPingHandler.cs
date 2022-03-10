using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Utilities;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using System;

namespace MqttFx.Client.Channels;

/// <summary>
/// ping 处理器
/// 客户端有责任确保发送的控制数据包之间的间隔不超过"保持活动状态"值。在不发送任何其他控制数据包的情况下，客户端必须发送 PINGREQ 数据包 [MQTT-3.1.2-23]。
/// 如果"保持活动状态"值不为零，并且服务器在"保持活动状态"时间段的一倍半内未收到来自客户端的控制数据包，则它必须断开与客户端的网络连接，就好像网络出现故障一样 [MQTT-3.1.2-24]。
/// </summary>
class MqttPingHandler : SimpleChannelInboundHandler<object>
{
    private readonly ushort keepAlive;
    private IScheduledTask pingRespTimeout;

    public MqttPingHandler(ushort keepAlive)
    {
        this.keepAlive = keepAlive;
    }

    protected override void ChannelRead0(IChannelHandlerContext ctx, object packet)
    {
        switch (packet)
        {
            case PingReqPacket:
                HandlePingReq(ctx.Channel);
                break;
            case PingRespPacket:
                HandlePingResp();
                break;
            default:
                ctx.FireChannelRead(ReferenceCountUtil.Retain(packet));
                break;
        }
    }

    public override void UserEventTriggered(IChannelHandlerContext ctx, object evt)
    {
        base.UserEventTriggered(ctx, evt);

        if (evt is IdleStateEvent evt2)
        {
            switch (evt2.State)
            {
                case IdleState.ReaderIdle:
                    break;
                case IdleState.WriterIdle:
                    SendPingReq(ctx.Channel);
                    break;
            }
        }
    }

    /// <summary>
    /// This Packet is used in Keep Alive processing
    /// 发送心跳包至server端，并建立心跳超时断开连接任务
    /// 此处，先行创建心跳超时任务，后续再发送心跳包(避免收到心跳响应时，心跳超时任务未建立完成)
    /// </summary>
    void SendPingReq(IChannel channel)
    {
        // 创建心跳超时，断开连接任务
        // 如果客户端在发送 PINGREQ 后的合理时间内未收到 PINGRESP 数据包，则应关闭与服务器的网络连接。
        // 服务端在1.5个时长内未收到PINGREQ，就断开连接。
        // 客户端在1个时长内未收到PINGRES，断开连接。
        if (pingRespTimeout == null)
        {
            pingRespTimeout = channel.EventLoop.Schedule(() =>
            {
                channel.WriteAndFlushAsync(DisconnectPacket.Instance);
            }, TimeSpan.FromSeconds(keepAlive));
        }

        channel.WriteAndFlushAsync(PingReqPacket.Instance);
    }

    /// <summary>
    /// 服务器必须发送 PINGRESP 数据包以响应 PINGREQ 数据包 [MQTT-3.12.4-1]。
    /// The Server MUST send a PINGRESP Packet in response to a PINGREQ Packet [MQTT-3.12.4-1].
    /// </summary>
    void HandlePingReq(IChannel channel)
    {
        channel.WriteAndFlushAsync(PingRespPacket.Instance);
    }

    /// <summary>
    /// 处理ping resp,取消ping超时任务(断开连接)
    /// </summary>
    void HandlePingResp()
    {
        if (pingRespTimeout != null)
        {
            pingRespTimeout.Cancel();
            pingRespTimeout = null;
        }
    }
}
