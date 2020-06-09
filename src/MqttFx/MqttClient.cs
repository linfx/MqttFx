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

namespace MqttFx
{
    /// <summary>
    /// Mqtt客户端
    /// </summary>
    public class MqttClient : IMqttClient
    {
        private readonly ILogger _logger;
        private readonly PacketIdProvider _packetIdProvider = new PacketIdProvider();
        private readonly PacketDispatcher _packetDispatcher = new PacketDispatcher();

        private IEventLoopGroup eventLoop;
        private volatile IChannel channel;

        public Action<ConnectReturnCode> OnConnected;
        public Action OnDisconnected;
        public Action<Message> OnMessageReceived;

        public MqttClient(
            ILogger<MqttClient> logger,
            IOptions<MqttClientOptions> options)
        {
            Config = options.Value;
            _logger = logger ?? NullLogger<MqttClient>.Instance;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public async ValueTask<MqttConnectResult> ConnectAsync()
        {
            if (eventLoop == null)
                eventLoop = new MultithreadEventLoopGroup();

            try
            {
                var connectFuture = new TaskCompletionSource<MqttConnectResult>();
                var bootstrap = new Bootstrap();
                bootstrap
                    .Group(eventLoop)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true)
                    .RemoteAddress(Config.Host, Config.Port)
                    .Handler(new MqttChannelInitializer(this, connectFuture));

                var future = await bootstrap.ConnectAsync();
                if (future.Open)
                {
                    _packetDispatcher.Reset();
                    _packetIdProvider.Reset();
                    channel = future;
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
        public Task PublishAsync(string topic, byte[] payload, MqttQos qos = MqttQos.AtMostOnce, bool retain = false)
        {
            var packet = new PublishPacket(qos, false, retain)
            {
                TopicName = topic,
                Payload = payload
            };
            if (qos > MqttQos.AtMostOnce)
                packet.PacketId = _packetIdProvider.NewPacketId();

            return SendAndFlushPacketAsync(packet);
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="qos">服务质量等级</param>
        /// <param name="cancellationToken"></param>
        public Task<SubAckPacket> SubscribeAsync(string topic, MqttQos qos, CancellationToken cancellationToken)
        {
            var packet = new SubscribePacket
            {
                PacketId = _packetIdProvider.NewPacketId(),
            };
            packet.Add(topic, qos);

            return SendAndReceiveAsync<SubAckPacket>(packet, cancellationToken);
        }

        ///// <summary>
        ///// 取消订阅
        ///// </summary>
        ///// <param name="topics">主题</param>
        //public Task<UnsubscribeAckMessage> UnsubscribeAsync(params string[] topics)
        //{
        //    var packet = new UnsubscribePacket();
        //    packet.AddRange(topics);

        //    return SendAndReceiveAsync<UnsubscribeAckMessage>(packet, _cancellationTokenSource.Token);
        //}

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            if (channel != null)
                await channel.CloseAsync();
            await eventLoop.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            OnDisconnected?.Invoke();
        }

        /// <summary>
        /// Subscribe on the given topic, with the given qos. When a message is received, MqttClient will invoke the <see cref="IMqttHandler"/> #OnMessage(string, byte[])} function of the given handler
        /// </summary>
        /// <param name="topic">The topic filter to subscribe to</param>
        /// <param name="handler">The handler to invoke when we receive a message</param>
        /// <param name="qos">The qos to request to the server</param>
        /// <returns></returns>
        public Task On(string topic, IMqttHandler handler, MqttQos qos = MqttQos.AtLeastOnce)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 配置
        /// </summary>
        public MqttClientOptions Config { get; }

        #region ==================== PRIVATE API ====================

        private async Task<TPacket> SendAndReceiveAsync<TPacket>(Packet packet, CancellationToken cancellationToken = default) where TPacket : Packet
        {
            cancellationToken.ThrowIfCancellationRequested();

            ushort identifier = 0;
            if (packet is PacketWithId packetWithId)
                identifier = packetWithId.PacketId;

            var awaiter = _packetDispatcher.AddPacketAwaiter<TPacket>(identifier);

            await channel.WriteAndFlushAsync(packet);

            using var timeoutCts = new CancellationTokenSource(Config.Timeout);
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

        private Task SendAndFlushPacketAsync(Packet packet)
        {
            if (channel == null)
                return Task.CompletedTask;

            if (channel.Active)
                return channel.WriteAndFlushAsync(packet);

            return Task.CompletedTask;
        }

        private Task CreateSubscription(string topic, IMqttHandler handler, MqttQos qos)
        {
            var packet = new SubscribePacket
            {
                PacketId = _packetIdProvider.NewPacketId(),
            };
            packet.Add(topic, qos);

            return SendAndReceiveAsync<SubAckPacket>(packet);
        }

        #endregion
    }
}
