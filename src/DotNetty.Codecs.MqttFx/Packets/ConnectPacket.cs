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

                FixedHeader.Encode(buffer, buf.ReadableBytes);
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
            VariableHeader.Decode(buffer, FixedHeader);
            Payload.Decode(buffer, FixedHeader, VariableHeader);
        }
    }
}
