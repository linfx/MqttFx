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
using MqttFx.Client.Channels;
using MqttFx.Formatter;
using MqttFx.Utils;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MqttFx.Client;

/// <summary>
/// Mqtt客户端
/// 使用 MQTT 的程序或设备。客户端始终建立与服务器的网络连接。
/// A program or device that uses MQTT. A Client always establishes the Network Connection to the Server.
/// # 发布其他客户端可能感兴趣的应用程序消息。( Publish Application Messages that other Clients might be interested in.)
/// # 发布其他客户端可能感兴趣的应用程序消息。( Subscribe to request Application Messages that it is interested in receiving.)
/// # 取消订阅以删除对应用程序消息的请求。(Unsubscribe to remove a request for Application Messages.)
/// # 断开与服务器的连接。(Disconnect from the Server.)
/// </summary>
public class MqttClient
{
    private readonly ILogger logger;
    public IEventLoopGroup EventLoop { get; private set; }
    private volatile IChannel channel;
    private readonly PacketIdProvider packetIdProvider = new();

    internal ConcurrentDictionary<int, PendingPublish> PendingPublishs = new();
    internal ConcurrentDictionary<int, PendingSubscription> PendingSubscriptions = new();
    internal ConcurrentDictionary<int, PendingUnSubscription> PendingUnSubscriptions = new();

    public bool IsConnected { get; private set; }

    public MqttClientOptions Options { get; }

    public event Func<ConnectResult, Task> ConnectedAsync;

    public event Func<Task> DisconnectedAsync;

    public event Func<ApplicationMessage, Task> ApplicationMessageReceivedAsync;

    public MqttClient(ILogger<MqttClient> logger, IOptions<MqttClientOptions> options)
    {
        this.logger = logger ?? NullLogger<MqttClient>.Instance;
        Options = options.Value;
    }

    /// <summary>
    /// 连接
    /// </summary>
    /// <returns></returns>
    public async ValueTask<ConnectResult> ConnectAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (EventLoop == null)
            EventLoop = new MultithreadEventLoopGroup();

        var connectFuture = new TaskCompletionSource<ConnectResult>();
        var bootstrap = new Bootstrap();
        bootstrap
            .Group(EventLoop)
            .Channel<TcpSocketChannel>()
            .Option(ChannelOption.TcpNodelay, true)
            .RemoteAddress(Options.Host, Options.Port)
            .Handler(new ActionChannelInitializer<ISocketChannel>(ch =>
            {
                ch.Pipeline.AddLast(new LoggingHandler());
                ch.Pipeline.AddLast(MqttEncoder.Instance, new MqttDecoder(false, 256 * 1024));
                ch.Pipeline.AddLast(new IdleStateHandler(Options.Timeout, Options.Timeout, TimeSpan.Zero), new MqttPingHandler(Options.KeepAlive));
                ch.Pipeline.AddLast(new MqttChannelHandler(this, connectFuture));
            }));

        try
        {
            channel = await bootstrap.ConnectAsync();
            if (channel.Open)
            {
                packetIdProvider.Reset();
                PendingSubscriptions.Clear();
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
    /// <param name="applicationMessage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<PublishResult> PublishAsync(ApplicationMessage applicationMessage, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var packet = PublishPacketFactory.Create(applicationMessage);
        if (packet.Qos > MqttQos.AtMostOnce)
            packet.PacketId = packetIdProvider.NewPacketId();

        SendAsync(packet);

        if (packet.Qos == MqttQos.AtMostOnce)
            return Task.FromResult(new PublishResult());
        else
        {
            PendingPublish pending = new(packet);
            PendingPublishs.TryAdd(packet.PacketId, pending);
            pending.StartPublishRetransmissionTimer(EventLoop.GetNext(), SendAsync);
            return pending.Future.Task;
        }
    }

    /// <summary>
    /// 订阅消息
    /// </summary>
    /// <param name="subscriptionRequests"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<SubscribeResult> SubscribeAsync(SubscriptionRequests subscriptionRequests, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var packet = new SubscribePacket(packetIdProvider.NewPacketId(), subscriptionRequests.Requests.ToArray());
        SendAsync(packet);

        PendingSubscription pending = new(packet);
        PendingSubscriptions.TryAdd(packet.PacketId, pending);
        pending.StartRetransmitTimer(EventLoop.GetNext(), SendAsync);
        return pending.Future.Task;
    }

    /// <summary>
    /// 取消订阅
    /// </summary>
    /// <param name="topicFilters">主题</param>
    public Task<UnSubscribeResult> UnsubscribeAsync(params string[] topicFilters)
    {
        var packet = new UnsubscribePacket(packetIdProvider.NewPacketId(), topicFilters);
        SendAsync(packet);

        PendingUnSubscription pending = new(packet);
        PendingUnSubscriptions.TryAdd(packet.PacketId, pending);
        pending.StartRetransmitTimer(EventLoop.GetNext(), SendAsync);
        return pending.Future.Task;
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    /// <returns></returns>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IsConnected = false;
        if (channel != null)
        {
            await SendAsync(new DisconnectPacket());
            await channel.CloseAsync();
        }
        await EventLoop.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
        await OnDisconnected();
    }

    /// <summary>
    /// 发送包
    /// </summary>
    /// <param name="packet"></param>
    /// <returns></returns>
    public Task SendAsync(Packet packet)
    {
        if (channel == null)
            return Task.CompletedTask;

        if (channel.Active)
            return channel.WriteAndFlushAsync(packet);

        return Task.CompletedTask;
    }

    internal Task OnConnected(ConnectResult result)
    {
        if (ConnectedAsync == null)
            return Task.CompletedTask;

        return ConnectedAsync(result);
    }

    internal Task OnDisconnected()
    {
        if (DisconnectedAsync == null)
            return Task.CompletedTask;

        return DisconnectedAsync();
    }

    internal Task OnApplicationMessageReceived(ApplicationMessage message)
    {
        if (ApplicationMessageReceivedAsync == null)
            return Task.CompletedTask;

        return ApplicationMessageReceivedAsync(message);
    }
}
