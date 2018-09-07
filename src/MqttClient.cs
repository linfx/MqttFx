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
    public class MqttClient : IDisposable
    {
        readonly ILogger _logger;
        private IChannel _clientChannel;
        public Action<Message> OnMessageReceived;

        public MqttClient(string clientId = default, ILogger logger = default)
        {
            ClientId = clientId ?? "Lin";
            _logger = logger ?? NullLogger<MqttClient>.Instance;
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
        public async Task ConnectAsync(string username = default, string password = default)
        {
            var group = new MultithreadEventLoopGroup();
            var readListeningHandler = new ReadListeningHandler();
            var bootstrap = new Bootstrap();
            bootstrap
                .Group(group)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    pipeline.AddLast(MqttEncoder.Instance, new MqttDecoder(), readListeningHandler);
                }));

            try
            {
                _clientChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("118.126.96.166"), 1883));
                await Task.WhenAll(RunMqttClientAsync(_clientChannel, readListeningHandler));
                await _clientChannel.CloseAsync();
            }
            finally
            {
                await group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            }
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        async Task RunMqttClientAsync(IChannel channel, ReadListeningHandler readListener)
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

            while (true)
            {
                if (await readListener.ReceiveAsync() is Packet packet)
                {
                    switch (packet)
                    {
                        case ConnAckPacket connAckPacket:
                            break;
                        case PublishPacket publishPacket:
                            OnMessageReceived?.Invoke(new Message
                            {
                                Topic = publishPacket.TopicName,
                                Payload = publishPacket.Payload,
                                Qos = publishPacket.FixedHeader.Qos,
                                Retain = publishPacket.FixedHeader.Retain
                            });
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="payload">数据</param>
        /// <param name="qos">服务质量等级</param>
        public void Publish(string topic, byte[] payload, MqttQos qos = MqttQos.AtMostOnce)
        {
            var packet = new PublishPacket(qos)
            {
                MessageIdentifier = 0,
                TopicName = topic,
                Payload = payload
            };
            _clientChannel.WriteAndFlushAsync(packet);
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="qos">服务质量等级</param>
        public void Subscribe(string topic, MqttQos qos = MqttQos.AtMostOnce)
        {
            var packet = new SubscribePacket();
            packet.Subscribe(topic, qos);
            _clientChannel.WriteAndFlushAsync(packet);
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

        public void Dispose()
        {
            _clientChannel.DisconnectAsync();
            GC.SuppressFinalize(this);
        }
    }
}
