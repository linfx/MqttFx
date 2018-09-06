using System;
using System.Threading;
using nMqtt.Packets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels.Sockets;
using System.Net;

namespace nMqtt
{
    /// <summary>
    /// Mqtt客户端
    /// </summary>
    public class MqttClient : IDisposable
    {
        readonly ILogger _logger;
        Timer _pingTimer;
        public Action<Packet> OnMessageReceived;

        IChannel _clientChannel;

        public MqttClient(string clientId = default, ILogger logger = default)
        {
            ClientId = clientId ?? "Lin";
            _logger = logger ?? NullLogger<MqttClient>.Instance;
        }

        /// <summary>
        /// 客户端标识
        /// </summary>
        public string ClientId { get; set; }

        public short KeepAlive { get; set; } = 60;

        public bool CleanSession { get; set; } = true;

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
                await Task.WhenAll(ProcesMessageAsync(_clientChannel, readListeningHandler));
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
        async Task ProcesMessageAsync(IChannel channel, ReadListeningHandler readListener)
        {
            var connectPacket = new ConnectPacket
            {
                ClientId = ClientId,
                CleanSession = CleanSession
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
            connectPacket.KeepAlive = KeepAlive;
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

                            Console.WriteLine(publishPacket.TopicName);
                            //Console.WriteLine(publishPacket.Payload.ToString(System.Text.Encoding.UTF8));

                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="data">数据</param>
        /// <param name="qos">服务质量等级</param>
        public void Publish(string topic, byte[] data, MqttQos qos = MqttQos.AtMostOnce)
        {
            var msg = new PublishPacket();
            msg.FixedHeader.Qos = qos;
            msg.MessageIdentifier = 0;
            msg.TopicName = topic;
            msg.Payload = data;
            _clientChannel.WriteAndFlushAsync(msg);
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="qos"></param>
        public void Subscribe(string topic, MqttQos qos = MqttQos.AtMostOnce)
        {
            var msg = new SubscribePacket();
            msg.Subscribe(topic, qos);
            _clientChannel.WriteAndFlushAsync(msg);
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="topic"></param>
        public void Unsubscribe(string topic)
        {
            var msg = new UnsubscribePacket();
            msg.FixedHeader.Qos = MqttQos.AtLeastOnce;
            msg.Unsubscribe(topic);
            _clientChannel.WriteAndFlushAsync(msg);
        }

        public void Dispose()
        {
            if (_pingTimer != null)
                _pingTimer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
