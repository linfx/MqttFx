using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 固定报头(Fixed header)
    /// </summary>
    public struct FixedHeader
    {
        /// <summary>
        /// 报文类型(MQTT Control Packet type)
        /// </summary>
        public PacketType PacketType { get; set; }

        /// <summary>
        /// 每个MQTT控制报文类型特定的标志(Flags)
        /// </summary>
        internal int Flags { get; set; }

        /// <summary>
        /// 剩余长度(Remaining Length)
        /// 表示当前报文剩余部分的字节数，包括可变报头和负载的数据。
        /// 剩余长度不包括用于编码剩余长度字段本身的字节数。
        /// </summary>
        public int RemaingLength { internal get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="remaingLength">剩余长度</param>
        public void Encode(IByteBuffer buffer, int remaingLength = default)
        {
            if(remaingLength != default)
                RemaingLength = remaingLength;

            /*
             * MQTT控制报文的类型
             * 标志位 Header Flags
            */
            var headerFlags = (byte)PacketType << 4;
            headerFlags |= Flags;
            buffer.WriteByte(headerFlags); 

            /*
             * 剩余长度 Remaining Length
             * 剩余长度字段使用一个变长度编码方案，对小于128的值它使用单字节编码。
             * 更大的值按下面的方式处理。低7位有效位用于编码数据，最高有效位用于指示是否有更多的字节。
             * 因此每个字节可以编码128个数值和一个延续位（continuation bit）。
             * 剩余长度字段最大4个字节。
            */
            do
            {
                var digit = (byte)(remaingLength % 0x80);
                remaingLength /= 0x80;
                if (remaingLength > 0)
                    digit |= 0x80;
                buffer.WriteByte(digit);
            } while (remaingLength > 0);
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="buffer"></param>
        public void Decode(IByteBuffer buffer)
        {
            /*
             * MQTT控制报文的类型
             * 标志位 Header Flags
            */
            int headerFlags = buffer.ReadByte();
            PacketType = (PacketType)(headerFlags >> 4);
            Flags = headerFlags & 0x0f;

            /*
             * 剩余长度 Remaining Length
            */
            int multiplier = 1;
            short digit;
            int loops = 0;
            do
            {
                digit = buffer.ReadByte();
                RemaingLength += (digit & 127) * multiplier;
                multiplier *= 128;
                loops++;
            } while ((digit & 128) != 0 && loops < 4);

            // MQTT protocol limits Remaining Length to 4 bytes
            if (loops == 4 && (digit & 128) != 0)
                throw new DecoderException("remaining length exceeds 4 digits (" + PacketType + ')');
        }
    }
}