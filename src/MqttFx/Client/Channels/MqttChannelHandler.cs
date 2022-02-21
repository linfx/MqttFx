using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;
using MqttFx.Client;
using System.Threading.Tasks;

namespace MqttFx.Channels
{
    /// <summary>
    /// 发送和接收数据处理器
    /// </summary>
    public class MqttChannelHandler : SimpleChannelInboundHandler<Packet>
    {
        private readonly IMqttClient client;
        private readonly TaskCompletionSource<MqttConnectResult> connectFuture;

        public MqttChannelHandler(IMqttClient client, TaskCompletionSource<MqttConnectResult> connectFuture)
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
            if (client.Options.WillMessage != null)
            {
                variableHeader.ConnectFlags.WillFlag = true;
                variableHeader.ConnectFlags.WillQos = client.Options.WillMessage.Qos;
                variableHeader.ConnectFlags.WillRetain = client.Options.WillMessage.Retain;
                payload.WillTopic = client.Options.WillMessage.Topic;
                payload.WillMessage = client.Options.WillMessage.Payload;
            }
            context.WriteAndFlushAsync(packet);
        }

        /// <summary>
        /// 当收到对方发来的数据后，就会触发，参数msg就是发来的信息，可以是基础类型，也可以是序列化的复杂对象
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="msg"></param>
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
            var variableHeader = (ConnAckVariableHeader)message.VariableHeader;

            switch (variableHeader.ConnectReturnCode)
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
                    connectFuture.TrySetResult(new MqttConnectResult(variableHeader.ConnectReturnCode));
                    channel.CloseAsync();
                    break;
            }
        }

        private void ProcessMessage(IChannel channel, PublishPacket message)
        {
            switch (message.Qos)
            {
                case MqttQos.AT_MOST_ONCE:
                    InvokeProcessForIncomingPublish(message);
                    break;

                case MqttQos.AT_LEAST_ONCE:
                    InvokeProcessForIncomingPublish(message);
                    if (message.PacketIdentifier > 0)
                        channel.WriteAndFlushAsync(new PubAckPacket(message.PacketIdentifier));
                    break;

                case MqttQos.EXACTLY_ONCE:
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
