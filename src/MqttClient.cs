using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels.Sockets;
using nMqtt.Messages;
using nMqtt.Packets;
using nMqtt.Protocol;

namespace nMqtt
{
    /// <summary>
    /// Mqtt客户端
    /// </summary>
    public class MqttClient
    {
        readonly ILogger _logger;
        private IChannel _clientChannel;
        private IEventLoopGroup _group;
        public Action<Message> OnMessageReceived;
        public Action<ConnectReturnCode> OnConnected;

        public MqttClient(string clientId = default, ILogger logger = default)
        {
            ClientId = clientId ?? "Lin";
            _logger = logger ?? NullLogger<MqttClient>.Instance;
            _group = new MultithreadEventLoopGroup();
        }

        /// <summary>
        /// 客户端标识
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public async Task<ConnectReturnCode> ConnectAsync(string username = default, string password = default)
        {
            var readListeningHandler = new ReadListeningHandler();
            var bootstrap = new Bootstrap();
            bootstrap
                .Group(_group)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    pipeline.AddLast(MqttEncoder.Instance, new MqttDecoder(), readListeningHandler);
                }));

            try
            {
                _clientChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("118.126.96.166"), 1883)).ConfigureAwait(false);

                var connectResponse = await AuthenticateAsync(readListeningHandler).ConfigureAwait(false); ;
                if (connectResponse == ConnectReturnCode.ConnectionAccepted)
                {
                    StartReceivingPackets(readListeningHandler);
                }
                OnConnected?.Invoke(connectResponse);
                return connectResponse;
            }
            catch
            {
                await DisconnectAsync();
                return ConnectReturnCode.BrokerUnavailable;
            }
        }

        private async Task<ConnectReturnCode> AuthenticateAsync(ReadListeningHandler readListener)
        {
            var connectPacket = new ConnectPacket
            {
                ClientId = ClientId,
                CleanSession = true
            };
            //if (!string.IsNullOrEmpty(username))
            //{
            //    packet.UsernameFlag = true;
            //    packet.UserName = username;
            //}
            //if (!string.IsNullOrEmpty(password))
            //{
            //    packet.PasswordFlag = true;
            //    packet.Password = password;
            //}
            //connectPacket.KeepAlive = KeepAlive;

            await _clientChannel.WriteAndFlushAsync(connectPacket);
            if (await readListener.ReceiveAsync() is ConnAckPacket connAckPacket)
            {
                return connAckPacket.ConnectReturnCode;
            }
            return ConnectReturnCode.UnacceptedProtocolVersion;
        }

        private void StartReceivingPackets(ReadListeningHandler readListener)
        {
            Task.Run(() => ReceivePacketsAsync(readListener));
        }

        private async Task ReceivePacketsAsync(ReadListeningHandler readListener)
        {
            while (true)
            {
                if (await readListener.ReceiveAsync() is Packet packet)
                {
                    await ProcessReceivedPacketAsync(packet);
                }
            }
        }

        private Task ProcessReceivedPacketAsync(Packet packet)
        {
            switch (packet)
            {
                case DisconnectPacket disconnectPacket:
                    return DisconnectAsync();

                case PingReqPacket pingReqPacket:
                    return _clientChannel.WriteAndFlushAsync(new PingRespPacket());

                case PublishPacket publishPacket:
                    return ProcessReceivedPublishPacketAsync(publishPacket);

                case PublishRelPacket publishRelPacket:
                    return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }



        private Task ProcessReceivedPublishPacketAsync(PublishPacket publishPacket)
        {
            OnMessageReceived?.Invoke(new Message
            {
                Topic = publishPacket.TopicName,
                Payload = publishPacket.Payload,
                Qos = publishPacket.FixedHeader.Qos,
                Retain = publishPacket.FixedHeader.Retain
            });

            switch (publishPacket.FixedHeader.Qos)
            {
                case MqttQos.AtMostOnce:
                    return Task.CompletedTask;
                case MqttQos.AtLeastOnce:
                    return _clientChannel.WriteAndFlushAsync(new PublishAckPacket
                    {
                        MessageIdentifier = publishPacket.MessageIdentifier
                    });
                case MqttQos.ExactlyOnce:
                    return _clientChannel.WriteAndFlushAsync(new PublishRecPacket
                    {
                        MessageIdentifier = publishPacket.MessageIdentifier
                    });
                default:
                    throw new Exception("Received a not supported QoS level.");
            }
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="payload">有效载荷</param>
        /// <param name="qos">服务质量等级</param>
        public Task PublishAsync(string topic, byte[] payload, MqttQos qos = MqttQos.AtMostOnce)
        {
            var packet = new PublishPacket(qos)
            {
                MessageIdentifier = 0,
                TopicName = topic,
                Payload = payload
            };
            return _clientChannel.WriteAndFlushAsync(packet);
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="qos">服务质量等级</param>
        public Task SubscribeAsync(string topic, MqttQos qos = MqttQos.AtMostOnce)
        {
            var packet = new SubscribePacket();
            packet.Subscribe(topic, qos);
            return _clientChannel.WriteAndFlushAsync(packet);
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="topics">主题</param>
        public void Unsubscribe(params string[] topics)
        {
            var packet = new UnsubscribePacket();
            packet.AddRange(topics);
            _clientChannel.WriteAndFlushAsync(packet);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            await _clientChannel.CloseAsync();
            await _group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
        }
    }
}
