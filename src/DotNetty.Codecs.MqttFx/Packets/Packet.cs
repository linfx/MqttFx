using DotNetty.Buffers;
using System;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 消息基类
    /// </summary>
    public abstract class Packet
    {
        /// <summary>
        /// 固定报头
        /// </summary>
        public FixedHeader FixedHeader;

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
        public MqttQos Qos
        {
            get { return FixedHeader.Qos; }
            set { FixedHeader.Qos = value; }
        }

        /// <summary>
        /// 保留标志
        /// </summary>
        public bool Retain => FixedHeader.Retain;

        /// <summary>
        /// 剩余长度
        /// </summary>
        public int RemaingLength => FixedHeader.RemaingLength;

        /// <summary>
        /// 消息基类
        /// </summary>
        /// <param name="packetType"></param>
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
                WritePacketId(buf);
                FixedHeader.RemaingLength = buf.ReadableBytes;
                FixedHeader.Encode(buffer);
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
            ReadPacketId(buffer, ref remainingLength);
            FixedHeader.RemaingLength = remainingLength;
        }

        protected void WritePacketId(IByteBuffer buffer)
        {
            if (Qos > MqttQos.AtMostOnce)
                buffer.WriteUnsignedShort(PacketId);
        }

        protected void ReadPacketId(IByteBuffer buffer, ref int remainingLength)
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
}
