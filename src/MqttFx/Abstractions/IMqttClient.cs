using DotNetty.Codecs.MqttFx.Packets;
using System.Threading;
using System.Threading.Tasks;

namespace MqttFx
{
    public interface IMqttClient
    {
        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        Task<MqttConnectResult> ConnectAsync();

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="payload">有效载荷</param>
        /// <param name="qos">服务质量等级</param>
        /// <returns></returns>
        Task PublishAsync(string topic, byte[] payload, MqttQos qos = MqttQos.AtMostOnce, bool retain = false);

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="qos">服务质量等级</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<SubAckPacket> SubscribeAsync(string topic, MqttQos qos = MqttQos.AtMostOnce, CancellationToken cancellationToken = default);

        ///// <summary>
        ///// 取消订阅
        ///// </summary>
        ///// <param name="topics">主题</param>
        ///// <returns></returns>
        //Task<UnsubAckPacket> UnsubscribeAsync(params string[] topics);

        //bool IsConnected { get; }
        //event EventHandler<MqttClientConnectedEventArgs> Connected;
        //event EventHandler<MqttClientDisconnectedEventArgs> Disconnected;
        //event EventHandler<MqttMessageReceivedEventArgs> MessageReceived;
    }
}
