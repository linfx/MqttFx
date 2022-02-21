using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MqttFx.Channels;
using MqttFx.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MqttFx.Client
{
    /// <summary>
    /// Mqtt客户端
    /// </summary>
    public class MqttClient : IMqttClient
    {
        private readonly ILogger _logger;
        private IEventLoopGroup _eventLoop;
        private volatile IChannel _channel;
        private readonly PacketIdProvider _packetIdProvider = new PacketIdProvider();
        private readonly PacketDispatcher _packetDispatcher = new PacketDispatcher();

        public bool IsConnected { get; private set; }

        public MqttClientOptions Options { get; }

        public IMqttClientConnectedHandler ConnectedHandler { get; set; }

        public IMessageReceivedHandler MessageReceivedHandler { get; set; }

        public IMqttClientDisconnectedHandler DisconnectedHandler { get; set; }

        public MqttClient(ILogger<MqttClient> logger, IOptions<MqttClientOptions> options)
        {
            _logger = logger ?? NullLogger<MqttClient>.Instance;
            Options = options.Value;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public async ValueTask<MqttConnectResult> ConnectAsync(CancellationToken cancellationToken)
        {
            if (_eventLoop == null)
                _eventLoop = new MultithreadEventLoopGroup();

            var connectFuture = new TaskCompletionSource<MqttConnectResult>();
            var bootstrap = new Bootstrap();
            bootstrap
                .Group(_eventLoop)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .RemoteAddress(Options.Host, Options.Port)
                .Handler(new MqttChannelInitializer(this, connectFuture));

            try
            {
                _channel = await bootstrap.ConnectAsync();
                if (_channel.Open)
                {
                    _packetDispatcher.Reset();
                    _packetIdProvider.Reset();
                    IsConnected = true;
                }
                return await connectFuture.Task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new MqttException("BrokerUnavailable: " + ex.Message);
            }
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="payload">有效载荷</param>
        /// <param name="qos">服务质量等级</param>
        /// <param name="retain">保留消息</param>
        public Task PublishAsync(string topic, byte[] payload, MqttQos qos, bool retain, CancellationToken cancellationToken)
        {
            var packet = new PublishPacket(qos, false, retain)
            {
                TopicName = topic,
                //Payload = payload
            };
            if (qos > MqttQos.AT_MOST_ONCE)
                packet.PacketId = _packetIdProvider.NewPacketId();

            return SendPacketAsync(packet);
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="qos">服务质量等级</param>
        /// <param name="cancellationToken"></param>
        public Task SubscribeAsync(string topic, MqttQos qos, CancellationToken cancellationToken)
        {
            var packet = new SubscribePacket();
            packet.VariableHeader.PacketIdentifier = _packetIdProvider.NewPacketId();
            packet.Add(topic, qos);

            return SendPacketAsync(packet);
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="topics">主题</param>
        public Task UnsubscribeAsync(params string[] topics)
        {
            var packet = new UnsubscribePacket();
            packet.AddRange(topics);

            return SendPacketAsync(packet);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            if (_channel != null)
                await _channel.CloseAsync();

            await _eventLoop.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// 发送包
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        private Task SendPacketAsync(Packet packet)
        {
            if (_channel == null)
                return Task.CompletedTask;

            if (_channel.Active)
                return _channel.WriteAndFlushAsync(packet);

            return Task.CompletedTask;
        }
    }
}
