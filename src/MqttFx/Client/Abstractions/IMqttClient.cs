using DotNetty.Codecs.MqttFx.Packets;
using System.Threading;
using System.Threading.Tasks;

namespace MqttFx.Client.Abstractions
{
    /// <summary>
    /// Mqtt客户端
    /// </summary>
    public interface IMqttClient
    {
        /// <summary>
        /// 连接状态
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 配置
        /// </summary>
        MqttClientOptions Options { get; }

        /// <summary>
        /// 连接处理
        /// </summary>
        IMqttClientConnectedHandler ConnectedHandler { get; set; }

        /// <summary>
        /// 断开连接处理
        /// </summary>
        IMqttClientDisconnectedHandler DisconnectedHandler { get; set; }

        /// <summary>
        /// 收到消息处理
        /// </summary>
        IMessageReceivedHandler MessageReceivedHandler { get; set; }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        ValueTask<MqttConnectResult> ConnectAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="payload">有效载荷</param>
        /// <param name="qos">服务质量等级</param>
        /// <returns></returns>
        Task PublishAsync(string topic, byte[] payload, MqttQos qos = MqttQos.AtMostOnce, bool retain = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="qos">服务质量等级</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SubscribeAsync(string topic, MqttQos qos = MqttQos.AtMostOnce, CancellationToken cancellationToken = default);

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="topics">主题集合</param>
        /// <returns></returns>
        Task UnsubscribeAsync(params string[] topics);
    }
}
