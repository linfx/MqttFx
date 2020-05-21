using DotNetty.Buffers;
using System.Collections.Generic;

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

        public void WriteTo(IByteBuffer buffer)
        {
            var flags = (byte)PacketType << 4;
            flags |= Dup.ToByte() << 3;
            flags |= (byte)Qos << 1;
            flags |= Retain.ToByte();

            buffer.WriteByte((byte)flags);
            buffer.WriteBytes(EncodeLength(RemaingLength));
        }

        static byte[] EncodeLength(int length)
        {
            var result = new List<byte>();
            do
            {
                var digit = (byte)(length % 0x80);
                length /= 0x80;
                if (length > 0)
                    digit |= 0x80;
                result.Add(digit);
            } while (length > 0);

            return result.ToArray();
        }
    }
}
