using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;

namespace MqttFx.Transport
{
    public class MqttChannelHandler : SimpleChannelInboundHandler<Packet>
    {
        protected override void ChannelRead0(IChannelHandlerContext ctx, Packet msg)
        {
            switch (msg.PacketType)
            {
                case PacketType.CONNECT:
                    break;
                case PacketType.CONNACK:
                    break;
                case PacketType.PUBLISH:
                    HandlePublish(ctx.Channel, msg as PublishPacket);
                    break;
                case PacketType.PUBACK:
                    break;
                case PacketType.PUBREC:
                    break;
                case PacketType.PUBREL:
                    break;
                case PacketType.PUBCOMP:
                    break;
                case PacketType.SUBSCRIBE:
                    break;
                case PacketType.SUBACK:
                    break;
                case PacketType.UNSUBSCRIBE:
                    break;
                case PacketType.UNSUBACK:
                    break;
                case PacketType.PINGREQ:
                    break;
                case PacketType.PINGRESP:
                    break;
                case PacketType.DISCONNECT:
                    break;
                default:
                    break;
            }
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            base.ChannelActive(context);
        }

        private void HandleConack(IChannel channel)
        {
        }

        private void HandlePublish(IChannel channel, PublishPacket message)
        {
            switch (message.Qos)
            {
                case MqttQos.AtMostOnce:
                    break;

                case MqttQos.AtLeastOnce:
                    break;

                case MqttQos.ExactlyOnce:
                    break;
            }
        }
    }
}
