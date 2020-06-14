using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx.Packets
{
    public struct SubscribePayload
    {
        /// <summary>
        /// 主题列表
        /// </summary>
        public List<SubscriptionRequest> SubscribeTopics;
    }

    public class SubscriptionRequest
    {
        public SubscriptionRequest(string topic, MqttQos qos)
        {
            Topic = topic;
            Qos = qos;
        }

        public string Topic { get; set; }
        public MqttQos Qos { get; set; }
    }
}
