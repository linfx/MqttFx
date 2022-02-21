using DotNetty.Buffers;
using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx.Packets
{
    public class SubscribePayload : Payload
    {
        /// <summary>
        /// 主题列表
        /// </summary>
        public List<SubscribeRequest> SubscribeTopics = new List<SubscribeRequest>();

        public void Encode(IByteBuffer buffer)
        {
            foreach (var item in SubscribeTopics)
            {
                buffer.WriteString(item.Topic);
                buffer.WriteByte((byte)item.Qos);
            }
        }
    }

    public class SubscribeRequest
    {
        public SubscribeRequest(string topic, MqttQos qos)
        {
            Topic = topic;
            Qos = qos;
        }

        public string Topic { get; set; }
        public MqttQos Qos { get; set; }
    }
}
