using DotNetty.Buffers;
using DotNetty.Common.Utilities;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 连接回执
    /// </summary>
    public sealed class ConnAckPacket : Packet
    {
        public ConnAckPacket()
            : base(PacketType.CONNACK) { }

        /// <summary>
        /// 可变报头
        /// </summary>
        public ConnAckVariableHeader VariableHeader;

        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                buf.WriteByte(VariableHeader.SessionPresent ? 0x01 : 0x00);
                buf.WriteByte((byte)VariableHeader.ConnectReturnCode);

                FixedHeader.RemaingLength = buf.ReadableBytes;
                FixedHeader.Encode(buffer);
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
            int ackData = buffer.ReadUnsignedShort(ref remainingLength);
            VariableHeader.SessionPresent = ((ackData >> 8) & 0x1) != 0;
            VariableHeader.ConnectReturnCode = (ConnectReturnCode)(ackData & 0xFF);
            FixedHeader.RemaingLength = remainingLength;
        }
    }
}
