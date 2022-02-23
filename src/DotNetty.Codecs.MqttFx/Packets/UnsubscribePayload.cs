using DotNetty.Buffers;
using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 有效载荷(UNSUBSCRIBE Packet payload)
    /// The payload for the UNSUBSCRIBE Packet contains the list of Topic Filters that the Client wishes to unsubscribe from. The Topic Filters in an UNSUBSCRIBE packet MUST be UTF-8 encoded strings as defined in Section 1.5.3, packed contiguously [MQTT-3.10.3-1].
    /// The Payload of an UNSUBSCRIBE packet MUST contain at least one Topic Filter. An UNSUBSCRIBE packet with no payload is a protocol violation [MQTT-3.10.3-2]. 
    /// </summary>
    public class UnsubscribePayload : Payload
    {
        /// <summary>
        /// TopicFilters
        /// </summary>
        public string[] Topics { get; set; }

        public UnsubscribePayload() { }

        public UnsubscribePayload(string[] topics)
        {
            Topics = topics;
        }

        public override void Encode(IByteBuffer buffer, VariableHeader variableHeader)
        {
            foreach (var item in Topics)
            {
                buffer.WriteString(item);
            }
        }

        public override void Decode(IByteBuffer buffer, VariableHeader variableHeader, ref int remainingLength)
        {
            var unsubscribeTopics = new List<string>();
            while (remainingLength > 0)
            {
                string topicFilter = buffer.ReadString(ref remainingLength);
                ValidateTopicFilter(topicFilter);
                unsubscribeTopics.Add(topicFilter);
            }

            if (unsubscribeTopics.Count == 0)
                throw new DecoderException("[MQTT-3.10.3-2]");

            Topics = unsubscribeTopics.ToArray();
        }

        static void ValidateTopicFilter(string topicFilter)
        {
            int length = topicFilter.Length;
            if (length == 0)
                throw new DecoderException("[MQTT-4.7.3-1]");

            for (int i = 0; i < length; i++)
            {
                char c = topicFilter[i];
                switch (c)
                {
                    case '+':
                        if ((i > 0 && topicFilter[i - 1] != '/') || (i < length - 1 && topicFilter[i + 1] != '/'))
                        {
                            throw new DecoderException($"[MQTT-4.7.1-3]. Invalid topic filter: {topicFilter}");
                        }
                        break;
                    case '#':
                        if (i < length - 1 || (i > 0 && topicFilter[i - 1] != '/'))
                        {
                            throw new DecoderException($"[MQTT-4.7.1-2]. Invalid topic filter: {topicFilter}");
                        }
                        break;
                }
            }
        }
    }
}
