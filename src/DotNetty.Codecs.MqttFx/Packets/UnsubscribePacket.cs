using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 取消订阅(UNSUBSCRIBE – Unsubscribe from topics)
    /// An UNSUBSCRIBE Packet is sent by the Client to the Server, to unsubscribe from topics.
    /// </summary>
    public sealed class UnsubscribePacket : PacketWithId
    {
        public UnsubscribePacket()
            : this(new PacketIdVariableHeader(), new UnsubscribePayload(new List<string>())) { }

        public UnsubscribePacket(ushort packetId, params string[] topicFilters)
            : this(new PacketIdVariableHeader(packetId), new UnsubscribePayload(topicFilters)) { }

        public UnsubscribePacket(PacketIdVariableHeader variableHeader, UnsubscribePayload payload)
            : base(variableHeader, payload) { }

        public IList<string> TopicFilters
        {
            get => ((UnsubscribePayload)Payload).TopicFilters; 
            set => ((UnsubscribePayload)Payload).TopicFilters = value;
        }
    }
}
