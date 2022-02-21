using DotNetty.Codecs.MqttFx.Packets;

namespace MqttFx
{
    /// <summary>
    /// 消息
    /// </summary>
    public class Message
    {
        /// <summary>
        /// 主题
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// 有效载荷
        /// </summary>
        public byte[] Payload { get; set; }

        /// <summary>
        /// 服务质量等级
        /// </summary>
        public MqttQos Qos { get; set; }

        /// <summary>
        /// 保留标志
        /// </summary>
        public bool Retain { get; set; }
    }

    public static class MessageExtensions
    {
        public static Message ToMessage(this PublishPacket packet)
        {
            return new Message
            {
                //Qos = packet.FixedHeader.Qos,
                //Retain = packet.FixedHeader.Retain,
                //Topic = packet.VariableHeader.TopicName,
                //Payload = packet.Payload,
            };
        }
    }
}