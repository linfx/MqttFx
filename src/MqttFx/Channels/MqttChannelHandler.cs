using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;

namespace MqttFx.Channels
{
    public class MqttChannelHandler : SimpleChannelInboundHandler<Packet>
    {
        private readonly MqttClient client;
        private readonly MqttConnectResult connectFuture;

        public MqttChannelHandler(MqttClient client, MqttConnectResult connectFuture)
        {
            this.client = client;
            this.connectFuture = connectFuture;
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, Packet msg)
        {
            switch (msg.PacketType)
            {
                case PacketType.CONNACK:
                    HandleConack(ctx.Channel, (ConnAckPacket)msg);
                    break;
                case PacketType.SUBACK:
                    break;
                case PacketType.PUBLISH:
                    HandlePublish(ctx.Channel, (PublishPacket)msg);
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

        private void InvokeHandlersForIncomingPublish(PublishPacket message)
        {
        }

        private void HandleConack(IChannel channel, ConnAckPacket message)
        {
            switch (message.ConnectReturnCode)
            {
                case ConnectReturnCode.ConnectionAccepted:
                    connectFuture.Succeeded = true;
                    connectFuture.ConnectReturn = ConnectReturnCode.ConnectionAccepted;
                    break;

                case ConnectReturnCode.BadUsernameOrPassword:
                    //case CONNECTION_REFUSED_IDENTIFIER_REJECTED:
                    //case CONNECTION_REFUSED_NOT_AUTHORIZED:
                    //case CONNECTION_REFUSED_SERVER_UNAVAILABLE:
                    //case CONNECTION_REFUSED_UNACCEPTABLE_PROTOCOL_VERSION:
                    //    this.connectFuture.setSuccess(new MqttConnectResult(false, message.variableHeader().connectReturnCode(), channel.closeFuture()));
                    //    channel.close();
                    //    // Don't start reconnect logic here
                    break;
            }
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

        private void HandleSubAck(SubAckPacket message)
        {
        }

        private void HandleUnsuback(UnsubAckPacket message)
        {
        }

        private void HandlePuback(PubAckPacket message)
        {
        }

        private void HandlePubrec(IChannel channel, Packet message)
        {
        }

        private void HandlePubrel(IChannel channel, Packet message)
        {
        }

        private void HandlePubcomp(Packet message)
        {
        }
    }
}
