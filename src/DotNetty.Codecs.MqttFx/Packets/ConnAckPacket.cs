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
        /// 当前会话
        /// </summary>
        public bool SessionPresent { get; set; }

        /// <summary>
        /// 连接返回码
        /// </summary>
        public ConnectReturnCode ConnectReturnCode { get; set; }

        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                if (SessionPresent)
                    buf.WriteByte(1);  // 7 reserved 0-bits and SP = 1
                else
                    buf.WriteByte(0);  // 7 reserved 0-bits and SP = 0

                buf.WriteByte((byte)ConnectReturnCode);

                FixedHeader.RemaingLength = buf.ReadableBytes;
                FixedHeader.WriteFixedHeader(buffer);
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
            SessionPresent = ((ackData >> 8) & 0x1) != 0;
            ConnectReturnCode = (ConnectReturnCode)(ackData & 0xFF);
            FixedHeader.RemaingLength = remainingLength;
        }
    }
}
