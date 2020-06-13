using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 报文抽象类
    /// </summary>
    public abstract class Packet
    {
        /// <summary>
        /// 固定报头
        /// </summary>
        public FixedHeader FixedHeader;

        /// <summary>
        /// 报文抽象类
        /// </summary>
        /// <param name="packetType">报文类型</param>
        public Packet(PacketType packetType)
        {
            FixedHeader.PacketType = packetType;
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="buffer"></param>
        public virtual void Decode(IByteBuffer buffer) { }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="buffer"></param>
        public virtual void Encode(IByteBuffer buffer) { }
    }
}
