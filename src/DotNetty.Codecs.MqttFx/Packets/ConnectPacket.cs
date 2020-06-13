using DotNetty.Buffers;
using DotNetty.Common.Utilities;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 连接报文
    /// </summary>
    public sealed class ConnectPacket : Packet
    {
        /// <summary>
        /// 可变报头
        /// </summary>
        public ConnectVariableHeader VariableHeader;

        /// <summary>
        /// 有效载荷
        /// </summary>
        public ConnectPlayload Payload;

        /// <summary>
        /// 连接报文
        /// </summary>
        public ConnectPacket()
            : base(PacketType.CONNECT)
        {
            VariableHeader.ProtocolName = "MQTT";
            VariableHeader.ProtocolLevel = 0x04;
        }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="buffer"></param>
        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                VariableHeader.Encode(buf);
                Payload.Encode(buf, VariableHeader);
                FixedHeader.Encode(buffer, buf.WriterIndex);
                buffer.WriteBytes(buf);
            }
            finally
            {
                buf?.SafeRelease();
            }
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="buffer"></param>
        public override void Decode(IByteBuffer buffer)
        {
            int remainingLength = FixedHeader.RemaingLength;

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
                FixedHeader.Qos = (MqttQos)((connectFlags & 0x18) >> 3);
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
