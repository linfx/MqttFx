using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Utilities;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using System;

namespace MqttFx.Channels
{
    /// <summary>
    /// ping 处理器
    /// </summary>
    class MqttPingHandler : SimpleChannelInboundHandler<object>
    {
        //private int keepaliveSeconds;
        private IScheduledTask pingRespTimeout;

        protected override void ChannelRead0(IChannelHandlerContext ctx, object packet)
        {
            switch (packet)
            {
                case PingReqPacket:
                    HandlePingReq(ctx.Channel);
                    break;
                case PingRespPacket:
                    HandlePingResp(ctx.Channel);
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

        void SendPingReq(IChannel channel)
        {
            channel.WriteAndFlushAsync(PingReqPacket.Instance);

            if (pingRespTimeout != null)
            {
                pingRespTimeout = channel.EventLoop.Schedule(() =>
                {
                    channel.WriteAndFlushAsync(DisconnectPacket.Instance);
                }, TimeSpan.FromSeconds(10));
            }
        }

        void HandlePingReq(IChannel channel)
        {
            channel.WriteAndFlushAsync(PingRespPacket.Instance);
        }

        void HandlePingResp(IChannel channel)
        {
        }
    }
}
