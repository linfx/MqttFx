using DotNetty.Buffers;
using DotNetty.Common.Utilities;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 报文抽象类
    /// </summary>
    public abstract class Packet
    {
        /// <summary>
        /// 固定报头(Fixed header)
        /// </summary>
        public FixedHeader FixedHeader;

        /// <summary>
        /// 可变报头(Variable header)
        /// </summary>
        public VariableHeader VariableHeader;

        /// <summary>
        /// 有效载荷(Payload)
        /// </summary>
        public Payload Payload;

        /// <summary>
        /// 报文抽象类
        /// </summary>
        public Packet() { }

        /// <summary>
        /// 报文抽象类
        /// </summary>
        /// <param name="packetType">报文类型</param>
        public Packet(PacketType packetType)
        {
            FixedHeader.PacketType = packetType;
        }

        /// <summary>
        /// 报文抽象类
        /// </summary>
        /// <param name="fixedHeader"></param>
        /// <param name="variableHeader"></param>
        protected Packet(FixedHeader fixedHeader, VariableHeader variableHeader) :
            this(fixedHeader, variableHeader, null)
        {
        }

        /// <summary>
        /// 报文抽象类
        /// </summary>
        /// <param name="fixedHeader"></param>
        /// <param name="variableHeader"></param>
        protected Packet(FixedHeader fixedHeader, VariableHeader variableHeader, Payload payload)
        {
            FixedHeader = fixedHeader;
            VariableHeader = variableHeader;
            Payload = payload;
        }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="buffer"></param>
        public virtual void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                VariableHeader?.Encode(buf);
                Payload?.Encode(buf, VariableHeader);
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
        public virtual void Decode(IByteBuffer buffer)
        {
            VariableHeader?.Decode(buffer, FixedHeader);
            Payload?.Decode(buffer, FixedHeader, VariableHeader);
        }
    }
}
