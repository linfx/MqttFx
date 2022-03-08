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
using MqttFx.Formatter;
using MqttFx.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MqttFx.Client
{
    /// <summary>
    /// Mqtt客户端
    /// 使用 MQTT 的程序或设备。客户端始终建立与服务器的网络连接。
    /// A program or device that uses MQTT. A Client always establishes the Network Connection to the Server.
    /// </summary>
    public class MqttClient
    {
        private readonly ILogger logger;
        private IEventLoopGroup eventLoop;
        private volatile IChannel channel;
        private readonly PacketIdProvider packetIdProvider = new();
        private readonly PacketDispatcher packetDispatcher = new();

        public bool IsConnected { get; private set; }

        public MqttClientOptions Options { get; }

        internal IMqttClientConnectedHandler ConnectedHandler { get; set; }

        internal IMessageReceivedHandler MessageReceivedHandler { get; set; }

        internal IMqttClientDisconnectedHandler DisconnectedHandler { get; set; }

        public event Func<MqttConnectResult, Task> ConnectedAsync;

        internal Task OnConnected(MqttConnectResult result) => ConnectedAsync(result);

        public MqttClient(ILogger<MqttClient> logger, IOptions<MqttClientOptions> options)
        {
            this.logger = logger ?? NullLogger<MqttClient>.Instance;
            Options = options.Value;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public async ValueTask<MqttConnectResult> ConnectAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (eventLoop == null)
                eventLoop = new MultithreadEventLoopGroup();

            var connectFuture = new TaskCompletionSource<MqttConnectResult>();
            var bootstrap = new Bootstrap();
            bootstrap
                .Group(eventLoop)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .RemoteAddress(Options.Host, Options.Port)
                .Handler(new ActionChannelInitializer<ISocketChannel>(ch =>
                {
                    ch.Pipeline.AddLast(new LoggingHandler());
                    ch.Pipeline.AddLast(MqttEncoder.Instance, new MqttDecoder(false, 256 * 1024));
                    ch.Pipeline.AddLast(new IdleStateHandler(10, 10, 0), new MqttPingHandler());
                    ch.Pipeline.AddLast(new MqttChannelHandler(this, connectFuture));
                }));

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

        public Task PublishAsync(ApplicationMessage applicationMessage, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var packet = PublishPacketFactory.Create(applicationMessage);

            if (packet.Qos > MqttQos.AtMostOnce)
                packet.PacketId = packetIdProvider.NewPacketId();

            return SendAsync(packet, cancellationToken);
        }

        public Task SubscribeAsync(SubscriptionRequests subscriptionRequests, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var packet = new SubscribePacket(packetIdProvider.NewPacketId(), subscriptionRequests.Requests.ToArray());
            return SendAsync(packet);
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="topicFilters">主题</param>
        public Task UnsubscribeAsync(params string[] topicFilters)
        {
            var packet = new UnsubscribePacket(packetIdProvider.NewPacketId(), topicFilters);

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
        private Task SendAsync(Packet packet, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (channel == null)
                return Task.CompletedTask;

            if (channel.Active)
                return channel.WriteAndFlushAsync(packet);

            return Task.CompletedTask;
        }
    }
}
