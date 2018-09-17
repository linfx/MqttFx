using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using nMqtt.Extensions;
using nMqtt.Protocol;

namespace nMqtt.Packets
{
    /// <summary>
    /// 消息基类
    /// </summary>
    public abstract class Packet
    {
        #region FixedHeader

        /// <summary>
        /// 固定报头
        /// </summary>
        public FixedHeader FixedHeader { protected get; set; }

        /// <summary>
        /// 报文类型
        /// </summary>
        public PacketType PacketType => FixedHeader.PacketType;
        /// <summary>
        /// 重发标志
        /// </summary>
        public bool Dup => FixedHeader.Dup;
        /// <summary>
        /// 服务质量等级
        /// </summary>
        public MqttQos Qos => FixedHeader.Qos;
        /// <summary>
        /// 保留标志
        /// </summary>
        public bool Retain => FixedHeader.Retain;
        /// <summary>
        /// 剩余长度
        /// </summary>
        public int RemaingLength => FixedHeader.RemaingLength;

        #endregion

        public Packet()
        {
            var packetType = MqttPacketTypeProvider.GetPacketType(GetType());
            FixedHeader = new FixedHeader(packetType);
        }

        public Packet(PacketType msgType) => FixedHeader = new FixedHeader(msgType);

        public virtual void Encode(IByteBuffer buffer) { }

        public virtual void Decode(IByteBuffer buffer) { }


    }

    /// <summary>
    /// 消息基类(带ID)
    /// </summary>
    public abstract class PacketWithId : Packet
    {
        /// <summary>
        /// 报文标识符
        /// </summary>
        public ushort PacketId { get; set; }

        /// <summary>
        /// EncodePacketIdVariableHeader
        /// </summary>
        /// <param name="buffer"></param>
        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                EncodePacketId(buf);

                FixedHeader.RemaingLength = buf.ReadableBytes;
                FixedHeader.WriteTo(buffer);
                buffer.WriteBytes(buf);
                buf = null;
            }
            finally
            {
                buf?.Release();
            }
        }

        /// <summary>
        /// DecodePacketIdVariableHeader
        /// </summary>
        /// <param name="buffer"></param>
        public override void Decode(IByteBuffer buffer)
        {
            int remainingLength = RemaingLength;
            DecodePacketId(buffer, ref remainingLength);
            FixedHeader.RemaingLength = remainingLength;
        }

        protected void EncodePacketId(IByteBuffer buffer)
        {
            if (Qos > MqttQos.AtMostOnce)
            {
                buffer.WriteUnsignedShort(PacketId);
            }
        }

        protected void DecodePacketId(IByteBuffer buffer, ref int remainingLength)
        {
            if (Qos > MqttQos.AtMostOnce)
            {
                PacketId = buffer.ReadUnsignedShort(ref remainingLength);
                if (PacketId == 0)
                    throw new DecoderException("[MQTT-2.3.1-1]");
            }
        }
    }

    /// <summary>
    /// 固定报头
    /// </summary>
    public class FixedHeader
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

        public FixedHeader(PacketType packetType)
        {
            PacketType = packetType;
        }

        public FixedHeader(byte signature, int remainingLength)
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
