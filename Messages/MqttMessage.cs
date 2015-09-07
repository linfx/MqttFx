using System;
using System.Collections.Generic;
using System.IO;

namespace nMqtt.Messages
{
    /// <summary>
    /// Fixed header
    /// </summary>
    internal class FixedHeader
    {
        /// <summary>
        /// Message type
        /// </summary>
        public MessageType MessageType { get; set; }

        /// <summary>
        /// DUP flag
        /// </summary>
        public bool Dup { get; set; }

        /// <summary>
        /// QoS flags
        /// </summary>
        public Qos Qos { get; set; }

        /// <summary>
        /// RETAIN 保持
        /// </summary>
        public bool Retain { get; set; }

        /// <summary>
        /// 剩余长度
        /// </summary>
        public int RemaingLength { get; set; }

        public FixedHeader(MessageType msgType)
        {
            MessageType = msgType;
        }

        public FixedHeader(Stream stream)
        {
            if (stream.Length < 2)
                throw new Exception("The supplied header is invalid. Header must be at least 2 bytes long.");

            var byte1 = stream.ReadByte();
            MessageType = (MessageType)((byte1 & 0xf0) >> 4);
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

    internal class MqttMessage
    {
        public FixedHeader FixedHeader { get; protected set; }

        public MqttMessage(MessageType msgType)
        {
            FixedHeader = new FixedHeader(msgType);
        }

        public virtual void Encode(Stream stream)
        {
            FixedHeader.WriteTo(stream);
        }

        public virtual void Decode(Stream stream)
        {
        }

        public static MqttMessage DecodeMessage(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                return DecodeMessage(stream);
            }
        }

        public static MqttMessage DecodeMessage(Stream stream)
        {
            var header = new FixedHeader(stream);
            var msg = NewMessage(header.MessageType);
            msg.FixedHeader = header;
            msg.Decode(stream);
            return msg;
        }

        public static MqttMessage NewMessage(MessageType msgType)
        {
            switch (msgType)
            {
                case MessageType.CONNECT:
                    return new ConnectMessage();
                case MessageType.CONNACK:
                    return new ConnAckMessage();
                case MessageType.DISCONNECT:
                    return new DisconnectMessage();
                case MessageType.PINGREQ:
                    return new PingReqMessage();
                case MessageType.PINGRESP:
                    return new PingRespMessage();
                case MessageType.PUBACK:
                    return new PublishAckMessage();
                case MessageType.PUBCOMP:
                    return new PublishCompMessage();
                case MessageType.PUBLISH:
                    return new PublishMessage();
                case MessageType.PUBREC:
                    return new PublishRecMessage();
                case MessageType.PUBREL:
                    return new PublishRelMessage();
                case MessageType.SUBSCRIBE:
                    return new SubscribeMessage();
                case MessageType.SUBACK:
                    return new SubscribeAckMessage();
                case MessageType.UNSUBSCRIBE:
                    return new UnsubscribeMessage();
                case MessageType.UNSUBACK:
                    return new UnsubscribeMessage();
                default:
                    throw new Exception("Unsupported Message Type");
            }
        }
    }

    [Flags]
    public enum MessageType : byte
    {
        CONNECT     = 1,
        CONNACK     = 2,
        PUBLISH     = 3,
        PUBACK      = 4,
        PUBREC      = 5,
        PUBREL      = 6,
        PUBCOMP     = 7,
        SUBSCRIBE   = 8,
        SUBACK      = 9,
        UNSUBSCRIBE = 10,
        UNSUBACK    = 11,
        PINGREQ     = 12,
        PINGRESP    = 13,
        DISCONNECT  = 14
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
}
