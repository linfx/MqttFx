﻿using DotNetty.Buffers;
using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 有效载荷(SUBSCRIBE Packet payload)
    /// </summary>
    public class SubscribePayload : Payload
    {
        /// <summary>
        /// 主题列表
        /// </summary>
        public IList<TopicSubscription> TopicSubscriptions { get; set; }

        public SubscribePayload() { }

        public SubscribePayload(IList<TopicSubscription> topics)
        {
            TopicSubscriptions = topics;
        }

        public override void Encode(IByteBuffer buffer, VariableHeader variableHeader)
        {
            foreach (var item in TopicSubscriptions)
            {
                buffer.WriteString(item.TopicName);
                buffer.WriteByte((byte)item.Qos);
            }
        }

        public override void Decode(IByteBuffer buffer, VariableHeader variableHeader, ref int remainingLength)
        {
            TopicSubscriptions = new List<TopicSubscription>();
            while (remainingLength > 0)
            {
                string topicFilter = buffer.ReadString(ref remainingLength);
                MqttCodecUtil.ValidateTopicFilter(topicFilter);

                byte qos = buffer.ReadByte(ref remainingLength);
                if (qos > (byte)MqttQos.EXACTLY_ONCE)
                {
                    throw new DecoderException($"[MQTT-3.8.3-4]. Invalid QoS value: {qos}.");
                }

                TopicSubscription ts;
                ts.TopicName = topicFilter;
                ts.Qos = (MqttQos)qos;

                TopicSubscriptions.Add(ts);
            }

            if (TopicSubscriptions.Count == 0)
                throw new DecoderException("[MQTT-3.8.3-3]");
        }
    }
}