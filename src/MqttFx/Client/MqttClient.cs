using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MqttFx.Channels;
using MqttFx.Client.Abstractions;
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
        private IEventLoopGroup eventLoop;
        private volatile IChannel channel;
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
        public async ValueTask<MqttConnectResult> ConnectAsync(CancellationToken cancellationToken = default)
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
                    _packetDispatcher.Reset();
                    _packetIdProvider.Reset();
                    IsConnected = true;
                }
                return await connectFuture.Task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new MqttException("BrokerUnavailable");
            }
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="payload">有效载荷</param>
        /// <param name="qos">服务质量等级</param>
        /// <param name="retain"></param>
        public Task PublishAsync(string topic, byte[] payload, MqttQos qos = MqttQos.AtMostOnce, bool retain = false, CancellationToken cancellationToken = default)
        {
            var packet = new PublishPacket(qos, false, retain)
            {
                Payload = payload
            };
            packet.VariableHeader.TopicName = topic;
            if (qos > MqttQos.AtMostOnce)
                packet.VariableHeader.PacketIdentifier = _packetIdProvider.NewPacketId();

            return SendPacketAsync(packet);
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="qos">服务质量等级</param>
        /// <param name="cancellationToken"></param>
        public Task SubscribeAsync(string topic, MqttQos qos, CancellationToken cancellationToken = default)
        {
            var packet = new SubscribePacket();
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
            if (channel != null)
                await channel.CloseAsync();

            await eventLoop.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// 发送包
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        private Task SendPacketAsync(Packet packet)
        {
            if (channel == null)
                return Task.CompletedTask;

            if (channel.Active)
                return channel.WriteAndFlushAsync(packet);

            return Task.CompletedTask;
        }

        /// <summary>
        /// 发送包
        /// </summary>
        /// <typeparam name="TPacket"></typeparam>
        /// <param name="packet"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<TPacket> SendAndReceiveAsync<TPacket>(Packet packet, CancellationToken cancellationToken = default) where TPacket : Packet
        {
            cancellationToken.ThrowIfCancellationRequested();

            ushort identifier = 0;
            //if (packet is PacketWithIdentifier packetWithId)
            //    identifier = packetWithId.PacketId;

            var awaiter = _packetDispatcher.AddPacketAwaiter<TPacket>(identifier);

            await channel.WriteAndFlushAsync(packet);

            using var timeoutCts = new CancellationTokenSource(Options.Timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            linkedCts.Token.Register(() =>
            {
                if (!awaiter.Task.IsCompleted && !awaiter.Task.IsFaulted && !awaiter.Task.IsCanceled)
                    awaiter.TrySetCanceled();
            });

            try
            {
                var result = await awaiter.Task.ConfigureAwait(false);
                timeoutCts.Cancel(false);
                return (TPacket)result;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, ex.Message);
                _packetDispatcher.RemovePacketAwaiter<TPacket>(identifier);

                if (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                    throw new MqttTimeoutException(ex);
                else
                    throw;
            }
        }
    }
}
