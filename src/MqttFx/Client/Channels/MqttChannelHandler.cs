using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;
using MqttFx.Client.Abstractions;
using System.Threading.Tasks;

namespace MqttFx.Channels
{
    public class MqttChannelHandler : SimpleChannelInboundHandler<Packet>
    {
        private readonly IMqttClient client;
        private readonly TaskCompletionSource<MqttConnectResult> connectFuture;

        public MqttChannelHandler(IMqttClient client, TaskCompletionSource<MqttConnectResult> connectFuture)
        {
            this.client = client;
            this.connectFuture = connectFuture;
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            var packet = new ConnectPacket
            {
                ClientId = client.Options.ClientId,
                CleanSession = client.Options.CleanSession,
                KeepAlive = client.Options.KeepAlive,
            };
            if (client.Options.Credentials != null)
            {
                packet.UsernameFlag = true;
                packet.UserName = client.Options.Credentials.Username;
                packet.Password = client.Options.Credentials.Username;
            }
            if (client.Options.WillMessage != null)
            {
                packet.WillFlag = true;
                packet.WillQos = client.Options.WillMessage.Qos;
                packet.WillRetain = client.Options.WillMessage.Retain;
                packet.WillTopic = client.Options.WillMessage.Topic;
                packet.WillMessage = client.Options.WillMessage.Payload;
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
                case PacketType.DISCONNECT:
                    break;
            }
        }

        private void HandleConack(IChannel channel, ConnAckPacket message)
        {
            switch (message.ConnectReturnCode)
            {
                case ConnectReturnCode.ConnectionAccepted:
                    connectFuture.TrySetResult(new MqttConnectResult(ConnectReturnCode.ConnectionAccepted));

                    if(client.ConnectedHandler != null)
                        client.ConnectedHandler.OnConnected();
                    break;

                case ConnectReturnCode.BadUsernameOrPassword:
                case ConnectReturnCode.IdentifierRejected:
                case ConnectReturnCode.RefusedNotAuthorized:
                case ConnectReturnCode.BrokerUnavailable:
                case ConnectReturnCode.UnacceptableProtocolVersion:
                    connectFuture.TrySetResult(new MqttConnectResult(message.ConnectReturnCode));
                    channel.CloseAsync();
                    break;
            }
        }

        private void HandlePublish(IChannel channel, PublishPacket message)
        {
            switch (message.Qos)
            {
                case MqttQos.AtMostOnce:
                    InvokeHandlersForIncomingPublish(message);
                    break;

                case MqttQos.AtLeastOnce:
                    InvokeHandlersForIncomingPublish(message);
                    if(message.PacketId > 0)
                        channel.WriteAndFlushAsync(new PubAckPacket(message.PacketId));
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

        private void InvokeHandlersForIncomingPublish(PublishPacket message)
        {
            var handler = client.MessageReceivedHandler;
            if(handler != null)
            {
                handler.OnMesage(message.ToMessage());
            }
        }
    }
}
