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
        public MqttFixedHeader FixedHeader { protected get; set; }

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

        /// <summary>
        /// 消息基类
        /// </summary>
        /// <param name="packetType"></param>
        public Packet(PacketType packetType) => FixedHeader = new MqttFixedHeader(packetType);

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
                FixedHeader.WriteFixedHeader(buffer);
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
