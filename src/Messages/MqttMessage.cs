using System;
using System.Collections.Generic;
using System.IO;

namespace nMqtt.Messages
{
    /// <summary>
    /// 固定报头
    /// </summary>
    public class FixedHeader
    {
        /// <summary>
        /// 报文类型
        /// </summary>
        public MqttMessageType MessageType { get; set; }
        /// <summary>
        /// 重发标志
        /// </summary>
        public bool Dup { get; set; }
        /// <summary>
        /// 服务质量等级
        /// </summary>
        public Qos Qos { get; set; }
        /// <summary>
        /// 保留标志
        /// </summary>
        public bool Retain { get; set; }
        /// <summary>
        /// 剩余长度
        /// </summary>
        public int RemaingLength { get; set; }

        public FixedHeader(MqttMessageType msgType)
        {
            MessageType = msgType;
        }

        public FixedHeader(Stream stream)
        {
            if (stream.Length < 2)
                throw new Exception("The supplied header is invalid. Header must be at least 2 bytes long.");

            var byte1 = stream.ReadByte();
            MessageType = (MqttMessageType)((byte1 & 0xf0) >> 4);
            Dup = ((byte1 & 0x08) >> 3) > 0;
            Qos = (Qos)((byte1 & 0x06) >> 1);
            Retain = (byte1 & 0x01) > 0;

            RemaingLength = DecodeLenght(stream);
        }

        public void WriteTo(Stream stream)
        {
            var flags = (byte)MessageType << 4;
            flags |= Dup.ToByte() << 3;
            flags |= (byte)Qos << 1;
            flags |= Retain.ToByte();

            stream.WriteByte((byte)flags);                
            stream.Write(EncodeLength(RemaingLength));   
        }

        internal static byte[] EncodeLength(int length)
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

        internal static int DecodeLenght(Stream stream)
        {
            byte encodedByte;
            var multiplier = 1;
            var remainingLength = 0;
            do
            {
                encodedByte = (byte)stream.ReadByte();
                remainingLength += (encodedByte & 0x7f) * multiplier;
                multiplier *= 0x80;
            } while ((encodedByte & 0x80) != 0);

            return remainingLength;
        }
    }

    /// <summary>
    /// 消息基类
    /// </summary>
    public abstract class MqttMessage
    {
        /// <summary>
        /// 固定报头
        /// </summary>
        public FixedHeader FixedHeader { get; set; }

        public MqttMessage()
        {
            var att = (MessageTypeAttribute)Attribute.GetCustomAttribute(GetType(), typeof(MessageTypeAttribute));
            FixedHeader = new FixedHeader(att.MessageType);
        }

        public MqttMessage(MqttMessageType msgType) => FixedHeader = new FixedHeader(msgType);

        public virtual void Encode(Stream stream) => FixedHeader.WriteTo(stream);

        public virtual void Decode(Stream stream) { }
    }

    /// <summary>
    /// 报文类型
    /// </summary>
    [Flags]
    public enum MqttMessageType : byte
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

    /// <summary>
    /// 服务质量等级
    /// </summary>
    [Flags]
    public enum Qos : byte
    {
        /// <summary>
        ///     QOS Level 0 - Message is not guaranteed delivery. No retries are made to ensure delivery is successful.
        /// </summary>
        AtMostOnce = 0,
        /// <summary>
        ///     QOS Level 1 - Message is guaranteed delivery. It will be delivered at least one time, but may be delivered
        ///     more than once if network errors occur.
        /// </summary>
        AtLeastOnce = 1,
        /// <summary>
        ///     QOS Level 2 - Message will be delivered once, and only once. Message will be retried until
        ///     it is successfully sent..
        /// </summary>
        ExactlyOnce = 2,
    }

    /// <summary>
    /// 报文类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageTypeAttribute : Attribute
    {
        public MessageTypeAttribute(MqttMessageType messageType)
        {
            MessageType = messageType;
        }

        /// <summary>
        /// 报文类型
        /// </summary>
        public MqttMessageType MessageType { get; set; }
    }
}
