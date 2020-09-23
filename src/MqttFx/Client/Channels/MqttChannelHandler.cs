using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;
using MqttFx.Client;
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
            var packet = new ConnectPacket();
            packet.Payload.ClientId = client.Options.ClientId;
            packet.VariableHeader.CleanSession = client.Options.CleanSession;
            packet.VariableHeader.KeepAlive = client.Options.KeepAlive;
            if (client.Options.Credentials != null)
            {
                packet.VariableHeader.UsernameFlag = true;
                packet.Payload.UserName = client.Options.Credentials.Username;
                packet.Payload.Password = client.Options.Credentials.Username;
            }
            if (client.Options.WillMessage != null)
            {
                packet.VariableHeader.WillFlag = true;
                packet.VariableHeader.WillQos = client.Options.WillMessage.Qos;
                packet.VariableHeader.WillRetain = client.Options.WillMessage.Retain;
                packet.Payload.WillTopic = client.Options.WillMessage.Topic;
                packet.Payload.WillMessage = client.Options.WillMessage.Payload;
            }
            context.WriteAndFlushAsync(packet);
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, Packet msg)
        {
            switch (msg.FixedHeader.PacketType)
            {
                case PacketType.CONNACK:
                    ProcessMessage(ctx.Channel, (ConnAckPacket)msg);
                    break;
                case PacketType.PUBLISH:
                    ProcessMessage(ctx.Channel, (PublishPacket)msg);
                    break;
                case PacketType.PUBACK:
                    ProcessMessage(msg as PubAckPacket);
                    break;
                case PacketType.PUBREC:
                    ProcessMessage(ctx.Channel, msg as PubRecPacket);
                    break;
                case PacketType.PUBREL:
                    ProcessMessage(ctx.Channel, msg as PubRelPacket);
                    break;
                case PacketType.PUBCOMP:
                    ProcessMessage(msg);
                    break;
                case PacketType.SUBSCRIBE:
                    break;
                case PacketType.SUBACK:
                    ProcessMessage((SubAckPacket)msg);
                    break;
                case PacketType.UNSUBSCRIBE:
                    break;
                case PacketType.UNSUBACK:
                    break;
                case PacketType.DISCONNECT:
                    break;
            }
        }

        private void ProcessMessage(IChannel channel, ConnAckPacket message)
        {
            switch (message.VariableHeader.ConnectReturnCode)
            {
                case ConnectReturnCode.CONNECTION_ACCEPTED:
                    connectFuture.TrySetResult(new MqttConnectResult(ConnectReturnCode.CONNECTION_ACCEPTED));

                    if (client.ConnectedHandler != null)
                        client.ConnectedHandler.OnConnected();
                    break;

                case ConnectReturnCode.CONNECTION_REFUSED_BAD_USER_NAME_OR_PASSWORD:
                case ConnectReturnCode.CONNECTION_REFUSED_IDENTIFIER_REJECTED:
                case ConnectReturnCode.CONNECTION_REFUSED_SERVER_UNAVAILABLE:
                case ConnectReturnCode.CONNECTION_REFUSED_UNACCEPTABLE_PROTOCOL_VERSION:
                    connectFuture.TrySetResult(new MqttConnectResult(message.VariableHeader.ConnectReturnCode));
                    channel.CloseAsync();
                    break;
            }
        }

        private void ProcessMessage(IChannel channel, PublishPacket message)
        {
            switch (message.Qos)
            {
                case MqttQos.AtMostOnce:
                    InvokeProcessForIncomingPublish(message);
                    break;

                case MqttQos.AtLeastOnce:
                    InvokeProcessForIncomingPublish(message);
                    if (message.PacketIdentifier > 0)
                        channel.WriteAndFlushAsync(new PubAckPacket(message.PacketIdentifier));
                    break;

                case MqttQos.ExactlyOnce:
                    break;
            }
        }

        private void ProcessMessage(PubAckPacket message)
        {
        }

        private void ProcessMessage(IChannel channel, PubRecPacket message)
        {
            var packet = new PubRelPacket(message.VariableHeader.PacketIdentifier);
            channel.WriteAndFlushAsync(packet);
        }

        private void ProcessMessage(IChannel channel, PubRelPacket message)
        {
        }

        private void ProcessMessage(SubAckPacket message)
        {
        }

        private void ProcessMessage(UnsubAckPacket message)
        {
        }


        private void ProcessMessage(Packet message)
        {
        }

        private void InvokeProcessForIncomingPublish(PublishPacket message)
        {
            var handler = client.MessageReceivedHandler;
            if(handler != null)
            {
                handler.OnMesage(message.ToMessage());
            }
        }
    }
}
