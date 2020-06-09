using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 固定报头
    /// </summary>
    public class MqttFixedHeader
    {
        /// <summary>
        /// 报文类型
        /// </summary>
        public PacketType PacketType { get; set; }

        /// <summary>
        /// 重发标志
        /// </summary>
        public bool Dup { get; set; }

        /// <summary>
        /// 服务质量等级
        /// </summary>
        public MqttQos Qos { get; set; }

        /// <summary>
        /// 保留标志
        /// </summary>
        public bool Retain { get; set; }

        /// <summary>
        /// 剩余长度
        /// </summary>
        public int RemaingLength { internal get; set; }

        public MqttFixedHeader(PacketType packetType)
        {
            PacketType = packetType;
        }

        public MqttFixedHeader(byte signature, int remainingLength)
        {
            PacketType = (PacketType)((signature & 0xf0) >> 4);
            Dup = ((signature & 0x08) >> 3) > 0;
            Qos = (MqttQos)((signature & 0x06) >> 1);
            Retain = (signature & 0x01) > 0;
            RemaingLength = remainingLength;
        }

        /// <summary>
        /// 写入固定报头数据
        /// </summary>
        /// <param name="buf"></param>
        public void WriteTo(IByteBuffer buf)
        {
            buf.WriteByte(GetFixedHeaderByte());
            WriteVariableLength(buf, RemaingLength);
        }

        private int GetFixedHeaderByte()
        {
            int flags = (byte)PacketType << 4;
            flags |= Dup.ToByte() << 3;
            flags |= (byte)Qos << 1;
            flags |= Retain.ToByte();
            return flags;
        }

        private static void WriteVariableLength(IByteBuffer buf, int num)
        {
            do
            {
                var digit = (byte)(num % 0x80);
                num /= 0x80;
                if (num > 0)
                    digit |= 0x80;
                buf.WriteByte(digit);
            } while (num > 0);
        }
    }
}
