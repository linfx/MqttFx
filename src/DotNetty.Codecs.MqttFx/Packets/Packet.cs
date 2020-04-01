using DotNetty.Buffers;
using System;
using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx.Packets
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

        public Packet(PacketType packetType) => FixedHeader = new FixedHeader(packetType);

        public virtual void Decode(IByteBuffer buffer) { }

        public virtual void Encode(IByteBuffer buffer) { }
    }

    /// <summary>
    /// 消息基类(带ID)
    /// </summary>
    public abstract class PacketWithId : Packet
    {
        public PacketWithId(PacketType packetType)
            : base(packetType)
        {
        }

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
                buffer.WriteUnsignedShort(PacketId);
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

    /// <summary>
    /// 服务质量等级
    /// </summary>
    [Flags]
    public enum MqttQos : byte
    {
        /// <summary>
        /// QOS Level 0 - Message is not guaranteed delivery. No retries are made to ensure delivery is successful.
        /// </summary>
        AtMostOnce = 0x00,

        /// <summary>
        /// QOS Level 1 - Message is guaranteed delivery. It will be delivered at least one time, but may be delivered
        /// more than once if network errors occur.
        /// </summary>
        AtLeastOnce = 0x01,

        /// <summary>
        /// QOS Level 2 - Message will be delivered once, and only once. Message will be retried until
        /// it is successfully sent..
        /// </summary>
        ExactlyOnce = 0x02,

        /// <summary>
        /// Failure
        /// </summary>
        Failure = 0x80
    }

    /// <summary>
    /// 报文类型
    /// </summary>
    [Flags]
    public enum PacketType : byte
    {
        /// <summary>
        /// 发起连接
        /// </summary>
        CONNECT = 1,
        /// <summary>
        /// 连接回执
        /// </summary>
        CONNACK = 2,
        /// <summary>
        /// 发布消息
        /// </summary>
        PUBLISH = 3,
        /// <summary>
        /// 发布回执
        /// </summary>
        PUBACK = 4,
        /// <summary>
        /// QoS2消息回执
        /// </summary>
        PUBREC = 5,
        /// <summary>
        /// QoS2消息释放
        /// </summary>
        PUBREL = 6,
        /// <summary>
        /// QoS2消息完成
        /// </summary>
        PUBCOMP = 7,
        /// <summary>
        /// 订阅主题
        /// </summary>
        SUBSCRIBE = 8,
        /// <summary>
        /// 订阅回执
        /// </summary>
        SUBACK = 9,
        /// <summary>
        /// 取消订阅
        /// </summary>
        UNSUBSCRIBE = 10,
        /// <summary>
        /// 取消订阅回执
        /// </summary>
        UNSUBACK = 11,
        /// <summary>
        /// PING请求
        /// </summary>
        PINGREQ = 12,
        /// <summary>
        /// PING响应
        /// </summary>
        PINGRESP = 13,
        /// <summary>
        /// 断开连接
        /// </summary>
        DISCONNECT = 14
    }
}
