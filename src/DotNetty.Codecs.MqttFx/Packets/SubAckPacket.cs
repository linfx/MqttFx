using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 订阅回执(SUBACK – Subscribe acknowledgement)
    /// </summary>
    public class SubAckPacket : PacketWithId
    {
        public SubAckPacket()
            : this(new PacketIdVariableHeader(), new SubAckPayload(new List<MqttQos>())) { }

        public SubAckPacket(ushort packetId, params MqttQos[] returnCodes)
            : this(new PacketIdVariableHeader(packetId), new SubAckPayload(returnCodes)) { }

        public SubAckPacket(PacketIdVariableHeader variableHeader, SubAckPayload payload)
            : base(variableHeader, payload)
        {
            VariableHeader = variableHeader;
            Payload = payload;
        }

        /// <summary>
        /// 返回代码
        /// </summary>
        public IList<MqttQos> ReturnCodes
        {
            get { return ((SubAckPayload)Payload).ReturnCodes; }
            set { ((SubAckPayload)Payload).ReturnCodes = value; }
        }
    }
}