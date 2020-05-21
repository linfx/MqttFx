using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MqttFx.Transport;
using MqttFx.Utils;
using System;
using System.Net;
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
        private readonly MqttClientOptions _options;
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
            _options = options.Value;
            _logger = logger ?? NullLogger<MqttClient>.Instance;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public async Task<MqttConnectResult> ConnectAsync()
        {
            if (eventLoop == null)
                eventLoop = new MultithreadEventLoopGroup();

            var connectFuture = new MqttConnectResult();
            var bootstrap = new Bootstrap();
            bootstrap
                .Group(eventLoop)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new MqttChannelInitializer());

            try
            {
                _packetDispatcher.Reset();
                _packetIdProvider.Reset();
                var future = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(_options.Host), _options.Port));

                if (future.Active)
                    channel = future;
                
                //_packetReceiverTask = Task.Run(() => TryReceivePacketsAsync(clientReadListener, _cancellationTokenSource.Token));

                //var connectResponse = await AuthenticateAsync().ConfigureAwait(false);
                //if (connectResponse.ConnectReturnCode == ConnectReturnCode.ConnectionAccepted)
                //    OnConnected?.Invoke(connectResponse.ConnectReturnCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new MqttException("BrokerUnavailable");
            }
            finally
            {
                await DisconnectAsync();
            }

            return connectFuture;
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

        #region ==================== PRIVATE API ====================

        /// <summary>
        /// 认证
        /// </summary>
        /// <returns></returns>
        private Task<ConnAckPacket> AuthenticateAsync()
        {
            var packet = new ConnectPacket
            {
                ClientId = _options.ClientId,
                CleanSession = _options.CleanSession,
                KeepAlive = _options.KeepAlive,
            };
            if (_options.Credentials != null)
            {
                packet.UsernameFlag = true;
                packet.UserName = _options.Credentials.Username;
                packet.Password = _options.Credentials.Username;
            }
            if (_options.WillMessage != null)
            {
                packet.WillFlag = true;
                packet.WillQos = _options.WillMessage.Qos;
                packet.WillRetain = _options.WillMessage.Retain;
                packet.WillTopic = _options.WillMessage.Topic;
                packet.WillMessage = _options.WillMessage.Payload;
            }
            return this.SendAndReceiveAsync<ConnAckPacket>(packet);
        }

        /// <summary>
        /// 读取Packet
        /// </summary>
        /// <param name="clientReadListener"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task TryReceivePacketsAsync(ReadListeningHandler clientReadListener, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (await clientReadListener.ReceiveAsync() is Packet packet)
                        await TryProcessReceivedPacketAsync(packet);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while receiving packets.");
            }
        }

        /// <summary>
        /// 处理Packet
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        private async Task TryProcessReceivedPacketAsync(Packet packet)
        {
            _logger.LogDebug("Try process received packet: {0}", packet.PacketType);

            try
            {
                if (packet is PingReqPacket)
                {
                    await channel.WriteAndFlushAsync(new PingRespPacket());
                    return;
                }

                if (packet is DisconnectPacket)
                {
                    await DisconnectAsync();
                    return;
                }

                if (packet is PubAckPacket)
                    return;

                if (packet is PublishPacket publishPacket)
                {
                    await ProcessReceivedPublishPacketAsync(publishPacket);
                    return;
                }

                if (packet is PubRecPacket pubRecPacket)
                {
                    await channel.WriteAndFlushAsync(new PubRelPacket(pubRecPacket.PacketId));
                    return;
                }

                if (packet is PubRelPacket pubRelPacket)
                {
                    await channel.WriteAndFlushAsync(new PubCompPacket(pubRelPacket.PacketId));
                    return;
                }

                _packetDispatcher.Dispatch(packet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while process packets.");
                _packetDispatcher.Dispatch(ex);
            }
        }

        private Task ProcessReceivedPublishPacketAsync(PublishPacket publishPacket)
        {
            OnMessageReceived?.Invoke(new Message
            {
                Topic = publishPacket.TopicName,
                Payload = publishPacket.Payload,
                Qos = publishPacket.Qos,
                Retain = publishPacket.Retain
            });

            return publishPacket.Qos switch
            {
                MqttQos.AtMostOnce => Task.CompletedTask,
                MqttQos.AtLeastOnce => channel.WriteAndFlushAsync(new PubAckPacket(publishPacket.PacketId)),
                MqttQos.ExactlyOnce => channel.WriteAndFlushAsync(new PubRecPacket(publishPacket.PacketId)),
                _ => throw new MqttException("Received a not supported QoS level."),
            };
        }

        internal async Task<TPacket> SendAndReceiveAsync<TPacket>(Packet packet, CancellationToken cancellationToken) where TPacket : Packet
        {
            cancellationToken.ThrowIfCancellationRequested();

            ushort identifier = 0;
            if (packet is PacketWithId packetWithId)
                identifier = packetWithId.PacketId;

            var awaiter = _packetDispatcher.AddPacketAwaiter<TPacket>(identifier);

            await channel.WriteAndFlushAsync(packet);

            using var timeoutCts = new CancellationTokenSource(_options.Timeout);
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

        class MqttChannelInitializer : ChannelInitializer<ISocketChannel>
        {
            protected override void InitChannel(ISocketChannel ch)
            {
                //ch.Pipeline.AddLast("mqttDecoder")
            }
        }

        #endregion
    }
}
