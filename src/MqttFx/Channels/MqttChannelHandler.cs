using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;
using System.Threading.Tasks;

namespace MqttFx.Channels
{
    public class MqttChannelHandler : SimpleChannelInboundHandler<Packet>
    {
        private readonly MqttClientOptions config;
        private readonly TaskCompletionSource<MqttConnectResult> connectPromise;

        public MqttChannelHandler(MqttClientOptions config, TaskCompletionSource<MqttConnectResult> connectPromise)
        {
            this.config = config;
            this.connectPromise = connectPromise;
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            var packet = new ConnectPacket
            {
                ClientId = config.ClientId,
                CleanSession = config.CleanSession,
                KeepAlive = config.KeepAlive,
            };
            if (config.Credentials != null)
            {
                packet.UsernameFlag = true;
                packet.UserName = config.Credentials.Username;
                packet.Password = config.Credentials.Username;
            }
            if (config.WillMessage != null)
            {
                packet.WillFlag = true;
                packet.WillQos = config.WillMessage.Qos;
                packet.WillRetain = config.WillMessage.Retain;
                packet.WillTopic = config.WillMessage.Topic;
                packet.WillMessage = config.WillMessage.Payload;
            }
            context.WriteAndFlushAsync(packet);
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, Packet msg)
        {
            switch (msg.PacketType)
            {
                case PacketType.CONNACK:
                    HandleConack(ctx.Channel, (ConnAckPacket)msg);
                    break;
                case PacketType.SUBACK:
                    HandleSubAck((SubAckPacket)msg);
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

        private void InvokeHandlersForIncomingPublish(PublishPacket message)
        {
        }

        private void HandleConack(IChannel channel, ConnAckPacket message)
        {
            switch (message.ConnectReturnCode)
            {
                case ConnectReturnCode.ConnectionAccepted:
                    connectPromise.TrySetResult(new MqttConnectResult(ConnectReturnCode.ConnectionAccepted));
                    break;

                case ConnectReturnCode.BadUsernameOrPassword:
                case ConnectReturnCode.IdentifierRejected:
                case ConnectReturnCode.RefusedNotAuthorized:
                case ConnectReturnCode.BrokerUnavailable:
                case ConnectReturnCode.UnacceptableProtocolVersion:
                    connectPromise.TrySetResult(new MqttConnectResult(message.ConnectReturnCode));
                    channel.CloseAsync();
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
