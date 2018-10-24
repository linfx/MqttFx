using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Codecs.MqttFx;
using DotNetty.Codecs.MqttFx.Packets;
using MqttFx.Extensions;

namespace MqttFx
{
    /// <summary>
    /// Mqtt客户端
    /// </summary>
    public class MqttClient
    {
        readonly ILogger _logger;
        readonly IEventLoopGroup _group;
        readonly MqttClientOptions _options;
        readonly PacketIdProvider _packetIdentifierProvider;
        readonly PacketDispatcher _packetDispatcher;

        private IChannel _clientChannel;
        private CancellationTokenSource _cancellationTokenSource;

        public event EventHandler<MqttClientConnectedEventArgs> Connected;
        public event EventHandler<MqttClientDisconnectedEventArgs> Disconnected;
        public event EventHandler<MqttMessageReceivedEventArgs> MessageReceived;

        public MqttClient(IOptions<MqttClientOptions> options,
            ILogger<MqttClient> logger = default)
        {
            _logger = logger ?? NullLogger<MqttClient>.Instance;
            _group = new MultithreadEventLoopGroup();
            _packetIdentifierProvider = new PacketIdProvider();
            _packetDispatcher = new PacketDispatcher();
            _options = options.Value;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public async Task<ConnectReturnCode> ConnectAsync()
        {
            var clientReadListener = new ReadListeningHandler();
            var bootstrap = new Bootstrap();
            bootstrap
                .Group(_group)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    pipeline.AddLast(MqttEncoder.Instance, new MqttDecoder(false, 256 * 1024), clientReadListener);
                }));

            try
            {
                _packetDispatcher.Reset();
                _packetIdentifierProvider.Reset();
                _cancellationTokenSource = new CancellationTokenSource();
                _clientChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(_options.Server), _options.Port));

                StartReceivingPackets(clientReadListener, _cancellationTokenSource.Token);

                var connectResponse = await AuthenticateAsync(clientReadListener, _cancellationTokenSource.Token); ;
                if (connectResponse.ConnectReturnCode == ConnectReturnCode.ConnectionAccepted)
                {
                    Connected.Invoke(this, new MqttClientConnectedEventArgs(connectResponse.SessionPresent));
                }
                return connectResponse.ConnectReturnCode;
            }
            catch
            {
                await DisconnectAsync();
                throw new MqttException("BrokerUnavailable");
            }
        }

        private Task<ConnAckPacket> AuthenticateAsync(ReadListeningHandler readListener, CancellationToken cancellationToken)
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
            if(_options.WillMessage != null)
            {
                packet.WillFlag = true;
                packet.WillQos = _options.WillMessage.Qos;
                packet.WillRetain = _options.WillMessage.Retain;
                packet.WillTopic = _options.WillMessage.Topic;
                packet.WillMessage = _options.WillMessage.Payload;
            }
            return SendAndReceiveAsync<ConnAckPacket>(packet, cancellationToken);
        }

        private void StartReceivingPackets(ReadListeningHandler clientReadListener, CancellationToken cancellationToken)
        {
            Task.Run(() => ReceivePacketsAsync(clientReadListener, cancellationToken));
        }

        private async Task ReceivePacketsAsync(ReadListeningHandler clientReadListener, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (await clientReadListener.ReceiveAsync() is Packet packet)
                {
                    await ProcessReceivedPacketAsync(packet);
                }
            }
        }

        private Task ProcessReceivedPacketAsync(Packet packet)
        {
            _logger.LogInformation("【ProcessReceivedPacketAsync】: " + packet.PacketType);

            if (packet is PingReqPacket)
                return _clientChannel.WriteAndFlushAsync(new PingRespPacket());

            if (packet is DisconnectPacket)
                return DisconnectAsync();

            if (packet is PubAckPacket)
                return Task.CompletedTask;

            if (packet is PublishPacket publishPacket)
                return ProcessReceivedPublishPacketAsync(publishPacket);

            if (packet is PubRecPacket pubRecPacket)
                return _clientChannel.WriteAndFlushAsync(new PubRelPacket(pubRecPacket.PacketId));

            if (packet is PubRelPacket pubRelPacket)
                return _clientChannel.WriteAndFlushAsync(new PubCompPacket(pubRelPacket.PacketId));

            return _packetDispatcher.Dispatch(packet);
        }

        private Task ProcessReceivedPublishPacketAsync(PublishPacket publishPacket)
        {
            OnMessageReceived(_options.ClientId, publishPacket.ToMessage());

            switch (publishPacket.Qos)
            {
                case MqttQos.AtMostOnce:
                    return Task.CompletedTask;
                case MqttQos.AtLeastOnce:
                    return _clientChannel.WriteAndFlushAsync(new PubAckPacket(publishPacket.PacketId));
                case MqttQos.ExactlyOnce:
                    return _clientChannel.WriteAndFlushAsync(new PubRecPacket(publishPacket.PacketId));
                default:
                    throw new MqttException("Received a not supported QoS level.");
            }
        }

        private async Task<TResponsePacket> SendAndReceiveAsync<TResponsePacket>(Packet requestPacket, CancellationToken cancellationToken) where TResponsePacket : Packet
        {
            cancellationToken.ThrowIfCancellationRequested();

            ushort identifier = 0;
            if (requestPacket is PacketWithId packetWithId)
            {
                identifier = packetWithId.PacketId;
            }

            var awaiter = _packetDispatcher.AddPacketAwaiter<TResponsePacket>(identifier);
            try
            {
                await _clientChannel.WriteAndFlushAsync(requestPacket);
                using (var timeoutCts = new CancellationTokenSource(_options.Timeout))
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token))
                {
                    linkedCts.Token.Register(() =>
                    {
                        if (!awaiter.Task.IsCompleted && !awaiter.Task.IsFaulted && !awaiter.Task.IsCanceled)
                            awaiter.TrySetCanceled();
                    });

                    try
                    {
                        var result = await awaiter.Task.ConfigureAwait(false);
                        timeoutCts.Cancel(false);
                        return (TResponsePacket)result;
                    }
                    catch (OperationCanceledException exception)
                    {
                        if (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                            throw new MqttTimeoutException(exception);
                        else
                            throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MqttException(ex.Message, ex);
            }
            finally
            {
                _packetDispatcher.RemovePacketAwaiter<TResponsePacket>(identifier);
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
                TopicName = topic,
                Payload = payload
            };
            if(qos > MqttQos.AtMostOnce)
                packet.PacketId = _packetIdentifierProvider.GetPacketId();

            return _clientChannel.WriteAndFlushAsync(packet);
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="qos">服务质量等级</param>
        public Task<SubAckPacket> SubscribeAsync(string topic, MqttQos qos = MqttQos.AtMostOnce)
        {
            var packet = new SubscribePacket
            {
                PacketId = _packetIdentifierProvider.GetPacketId(),
            };
            packet.Add(topic, qos);

            return SendAndReceiveAsync<SubAckPacket>(packet, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="topics">主题</param>
        public Task<UnsubAckPacket> UnsubscribeAsync(params string[] topics)
        {
            var packet = new UnsubscribePacket();
            packet.AddRange(topics);

            return SendAndReceiveAsync<UnsubAckPacket>(packet, _cancellationTokenSource.Token); ;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            await _clientChannel.CloseAsync();
            await _group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            //OnDisconnected?.Invoke();
        }

        void OnMessageReceived(string clientId, Message message)
        {
            MessageReceived?.Invoke(this, new MqttMessageReceivedEventArgs(clientId, message));
        }
    }
}
