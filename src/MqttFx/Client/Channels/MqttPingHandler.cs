using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Common.Utilities;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;

namespace MqttFx.Channels
{
    /// <summary>
    /// ping 处理器
    /// </summary>
    internal class MqttPingHandler : SimpleChannelInboundHandler<object>
    {
        protected override void ChannelRead0(IChannelHandlerContext ctx, object msg)
        {
            if (msg is Packet message)
            {
                switch (message.FixedHeader.PacketType)
                {
                    case PacketType.PINGREQ:
                        HandlePingReq(ctx.Channel);
                        break;
                    case PacketType.PINGRESP:
                        HandlePingResp();
                        break;
                    default:
                        ctx.FireChannelRead(ReferenceCountUtil.Retain(msg));
                        break;
                }
            }
            else
                ctx.FireChannelRead(msg);
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

        private void SendPingReq(IChannel channel)
        {
            channel.WriteAndFlushAsync(PingReqPacket.Instance);
        }

        private void HandlePingReq(IChannel channel)
        {
            channel.WriteAndFlushAsync(PingRespPacket.Instance);
        }

        private void HandlePingResp()
        {
        }
    }
}
