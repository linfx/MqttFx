using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 订阅回执(SUBACK – Subscribe acknowledgement)
    /// </summary>
    public class SubAckPacket : PacketWithId
    {
        /// <summary>
        /// 返回代码
        /// </summary>
        public IList<MqttQos> ReturnCodes
        {
            get { return ((SubAckPayload)Payload).ReturnCodes; }
            set { ((SubAckPayload)Payload).ReturnCodes = value; }
        }

        public SubAckPacket()
            : this(new PacketIdVariableHeader(), new SubAckPayload()) { }

        public SubAckPacket(PacketIdVariableHeader variableHeader, SubAckPayload payload)
            : base(variableHeader, payload)
        {
            FixedHeader.PacketType = PacketType.SUBACK;
            VariableHeader = variableHeader;
            Payload = payload;
        }
    }
}