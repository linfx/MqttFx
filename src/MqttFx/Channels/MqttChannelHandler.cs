using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;
using System.Threading.Tasks;

namespace MqttFx.Channels
{
    public class MqttChannelHandler : SimpleChannelInboundHandler<Packet>
    {
        private readonly IMqttClient client;
        private readonly TaskCompletionSource<MqttConnectResult> connectPromise;

        public MqttChannelHandler(IMqttClient client, TaskCompletionSource<MqttConnectResult> connectPromise)
        {
            this.client = client;
            this.connectPromise = connectPromise;
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            var packet = new ConnectPacket
            {
                ClientId = client.Config.ClientId,
                CleanSession = client.Config.CleanSession,
                KeepAlive = client.Config.KeepAlive,
            };
            if (client.Config.Credentials != null)
            {
                packet.UsernameFlag = true;
                packet.UserName = client.Config.Credentials.Username;
                packet.Password = client.Config.Credentials.Username;
            }
            if (client.Config.WillMessage != null)
            {
                packet.WillFlag = true;
                packet.WillQos = client.Config.WillMessage.Qos;
                packet.WillRetain = client.Config.WillMessage.Retain;
                packet.WillTopic = client.Config.WillMessage.Topic;
                packet.WillMessage = client.Config.WillMessage.Payload;
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
                    HandlePuback((PubAckPacket)msg);
                    break;
                case PacketType.PUBREC:
                    HandlePubrel(ctx.Channel, msg);
                    break;
                case PacketType.PUBREL:
                    HandlePubrec(ctx.Channel, msg);
                    break;
                case PacketType.PUBCOMP:
                    HandlePubcomp(msg);
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
            }
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
