using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 固定报头
    /// </summary>
    public struct FixedHeader
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

        public FixedHeader(byte signature, int remainingLength)
        {
            PacketType = (PacketType)((signature & 0xf0) >> 4);
            Dup = ((signature & 0x08) >> 3) > 0;
            Qos = (MqttQos)((signature & 0x06) >> 1);
            Retain = (signature & 0x01) > 0;
            RemaingLength = remainingLength;
        }

        /// <summary>
        /// 写入固定报头
        /// </summary>
        /// <param name="buf"></param>
        public void WriteFixedHeader(IByteBuffer buf)
        {
            WriteFixedHeaderByte(buf);
            WriteVariableLength(buf, RemaingLength);
        }

        /// <summary>
        /// 写入固定报头首字节
        /// </summary>
        /// <returns></returns>
        private void WriteFixedHeaderByte(IByteBuffer buf)
        {
            var ret = (byte)PacketType << 4;
            ret |= Dup.ToByte() << 3;
            ret |= (byte)Qos << 1;
            ret |= Retain.ToByte();
            buf.WriteByte(ret);
        }

        /// <summary>
        /// 写入剩余长度
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="num"></param>
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
