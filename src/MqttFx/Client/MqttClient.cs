using DotNetty.Codecs.MqttFx;
using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Timeout;
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
        private readonly ILogger logger;
        private IEventLoopGroup eventLoop;
        private volatile IChannel channel;
        private readonly PacketIdProvider packetIdProvider = new();
        private readonly PacketDispatcher packetDispatcher = new();

        public bool IsConnected { get; private set; }

        public MqttClientOptions Options { get; }

        public IMqttClientConnectedHandler ConnectedHandler { get; set; }

        public IMessageReceivedHandler MessageReceivedHandler { get; set; }

        public IMqttClientDisconnectedHandler DisconnectedHandler { get; set; }

        public MqttClient(ILogger<MqttClient> logger, IOptions<MqttClientOptions> options)
        {
            this.logger = logger ?? NullLogger<MqttClient>.Instance;
            Options = options.Value;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public async ValueTask<MqttConnectResult> ConnectAsync(CancellationToken cancellationToken)
        {
            if (eventLoop == null)
                eventLoop = new MultithreadEventLoopGroup();

            var connectFuture = new TaskCompletionSource<MqttConnectResult>();
            var bootstrap = new Bootstrap();
            bootstrap
                .Group(eventLoop)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .RemoteAddress(Options.Host, Options.Port)
                .Handler(new MqttChannelInitializer(this, connectFuture));

            try
            {
                channel = await bootstrap.ConnectAsync();
                if (channel.Open)
                {
                    packetDispatcher.Reset();
                    packetIdProvider.Reset();
                    IsConnected = true;
                }
                return await connectFuture.Task;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
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
            };
            ((PublishPayload)packet.Payload).Payload = payload;

            if (qos > MqttQos.AT_MOST_ONCE)
                packet.PacketId = packetIdProvider.NewPacketId();

            return SendAsync(packet);
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="qos">服务质量等级</param>
        /// <param name="cancellationToken"></param>
        public Task SubscribeAsync(string topic, MqttQos qos, CancellationToken cancellationToken)
        {
            var packet = new SubscribePacket
            {
                PacketId = packetIdProvider.NewPacketId()
            };
            packet.AddSubscription(topic, qos);

            return SendAsync(packet);
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="topics">主题</param>
        public Task UnsubscribeAsync(params string[] topics)
        {
            var packet = new UnsubscribePacket(packetIdProvider.NewPacketId(), topics);

            return SendAsync(packet);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            if (channel != null)
                await channel.CloseAsync();

            await eventLoop.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// 发送包
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        private Task SendAsync(Packet packet)
        {
            if (channel == null)
                return Task.CompletedTask;

            if (channel.Active)
                return channel.WriteAndFlushAsync(packet);

            return Task.CompletedTask;
        }

        class MqttChannelInitializer : ChannelInitializer<ISocketChannel>
        {
            private readonly IMqttClient client;
            private readonly TaskCompletionSource<MqttConnectResult> connectFuture;

            public MqttChannelInitializer(IMqttClient client, TaskCompletionSource<MqttConnectResult> connectFuture)
            {
                this.client = client;
                this.connectFuture = connectFuture;
            }

            protected override void InitChannel(ISocketChannel ch)
            {
                ch.Pipeline.AddLast(new LoggingHandler());
                ch.Pipeline.AddLast(MqttEncoder.Instance, new MqttDecoder(false, 256 * 1024));
                ch.Pipeline.AddLast(new IdleStateHandler(10, 10, 0), new MqttPingHandler());
                ch.Pipeline.AddLast(new MqttChannelHandler(client, connectFuture));
            }
        }
    }
}
