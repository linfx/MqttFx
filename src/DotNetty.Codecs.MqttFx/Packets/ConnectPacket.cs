using DotNetty.Buffers;
using DotNetty.Common.Utilities;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 发起连接
    /// </summary>
    public sealed class ConnectPacket : Packet
    {
        /// <summary>
        /// 可变报头
        /// </summary>
        public ConnectVariableHeader VariableHeader;

        /// <summary>
        /// 载荷
        /// </summary>
        public ConnectPlayload Payload;

        public ConnectPacket()
            : base(PacketType.CONNECT)
        {
            VariableHeader.ProtocolName = "MQTT";
            VariableHeader.ProtocolLevel = 0x04;
        }

        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                VariableHeader.Encode(buf);
                Payload.Encode(VariableHeader, buf);
                FixedHeader.Encode(buffer, buf.WriterIndex);
                buffer.WriteBytes(buf);
            }
            finally
            {
                buf?.SafeRelease();
            }
        }

        public override void Decode(IByteBuffer buffer)
        {
            int remainingLength = RemaingLength;

            // variable header
            VariableHeader.ProtocolName = buffer.ReadString(ref remainingLength);
            VariableHeader.ProtocolLevel = buffer.ReadByte();

            // connect flags                      //byte 10
            int connectFlags = buffer.ReadByte();
            VariableHeader.CleanSession = (connectFlags & 0x02) == 0x02;
            VariableHeader.WillFlag = (connectFlags & 0x04) == 0x04;
            if (VariableHeader.WillFlag)
            {
                VariableHeader.WillRetain = (connectFlags & 0x20) == 0x20;
                Qos = (MqttQos)((connectFlags & 0x18) >> 3);
                Payload.WillTopic = string.Empty;
            }

            // keep alive

            // payload
            Payload.ClientId = buffer.ReadString(ref remainingLength);
            if (VariableHeader.WillFlag)
            {
                Payload.WillTopic = buffer.ReadString(ref remainingLength);
                //WillMessage = buffer.ReadBytes
            }
        }
    }
}
