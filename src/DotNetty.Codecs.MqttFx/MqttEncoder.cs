using DotNetty.Buffers;
using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.MqttFx
{
    /// <summary>
    /// 编码器
    /// </summary>
    public sealed class MqttEncoder : MessageToMessageEncoder<Packet>
    {
        public static readonly MqttEncoder Instance = new MqttEncoder();

        protected override void Encode(IChannelHandlerContext context, Packet message, List<object> output) => DoEncode(context.Allocator, message, output);

        public static void DoEncode(IByteBufferAllocator bufferAllocator, Packet packet, List<object> output)
        {
            IByteBuffer buffer = bufferAllocator.Buffer();
            try
            {
                packet.Encode(buffer);
                output.Add(buffer);
                buffer = null;
            }
            finally
            {
                buffer?.SafeRelease();
            }
        }
    }

    /// <summary>
    /// DotNetty.Codecs.Mqtt
    /// </summary>
    public sealed class MqttEncoder2 : MessageToMessageEncoder<Packet>
    {
        public static readonly MqttEncoder Instance = new MqttEncoder();
        const int PacketIdLength = 2;
        const int StringSizeLength = 2;
        const int MaxVariableLength = 4;

        protected override void Encode(IChannelHandlerContext context, Packet message, List<object> output) => DoEncode(context.Allocator, message, output);

        public override bool IsSharable => true;

        /// <summary>
        ///     This is the main encoding method.
        ///     It's only visible for testing.
        ///     @param bufferAllocator Allocates ByteBuf
        ///     @param packet MQTT packet to encode
        ///     @return ByteBuf with encoded bytes
        /// </summary>
        internal static void DoEncode(IByteBufferAllocator bufferAllocator, Packet packet, List<object> output)
        {
            switch (packet.PacketType)
            {
                case PacketType.CONNECT:
                    EncodeConnectMessage(bufferAllocator, (ConnectPacket)packet, output);
                    break;
                case PacketType.CONNACK:
                    EncodeConnAckMessage(bufferAllocator, (ConnAckPacket)packet, output);
                    break;
                case PacketType.PUBLISH:
                    EncodePublishMessage(bufferAllocator, (PublishPacket)packet, output);
                    break;
                case PacketType.PUBACK:
                case PacketType.PUBREC:
                case PacketType.PUBREL:
                case PacketType.PUBCOMP:
                case PacketType.UNSUBACK:
                    EncodePacketWithIdOnly(bufferAllocator, (PacketWithId)packet, output);
                    break;
                case PacketType.SUBSCRIBE:
                    EncodeSubscribeMessage(bufferAllocator, (SubscribePacket)packet, output);
                    break;
                case PacketType.SUBACK:
                    EncodeSubAckMessage(bufferAllocator, (SubAckPacket)packet, output);
                    break;
                case PacketType.UNSUBSCRIBE:
                    EncodeUnsubscribeMessage(bufferAllocator, (UnsubscribePacket)packet, output);
                    break;
                case PacketType.PINGREQ:
                case PacketType.PINGRESP:
                case PacketType.DISCONNECT:
                    EncodePacketWithFixedHeaderOnly(bufferAllocator, packet, output);
                    break;
                default:
                    throw new ArgumentException("Unknown packet type: " + packet.PacketType, nameof(packet));
            }
        }

        static void EncodeConnectMessage(IByteBufferAllocator bufferAllocator, ConnectPacket packet, List<object> output)
        {
            int payloadBufferSize = 0;

            // Client id
            string clientId = packet.ClientId;
            Util.ValidateClientId(clientId);
            byte[] clientIdBytes = EncodeStringInUtf8(clientId);
            payloadBufferSize += StringSizeLength + clientIdBytes.Length;

            byte[] willTopicBytes;
            IByteBuffer willMessage;
            if (packet.WillFlag)
            {
                // Will topic and message
                string willTopic = packet.WillTopic;
                willTopicBytes = EncodeStringInUtf8(willTopic);

                //willMessage = packet.WillMessage;
                willMessage = Unpooled.Buffer();
                willMessage.WriteBytes(packet.WillMessage);

                payloadBufferSize += StringSizeLength + willTopicBytes.Length;
                payloadBufferSize += 2 + willMessage.ReadableBytes;
            }
            else
            {
                willTopicBytes = null;
                willMessage = null;
            }

            string userName = packet.UserName;
            byte[] userNameBytes;
            if (packet.UsernameFlag)
            {
                userNameBytes = EncodeStringInUtf8(userName);
                payloadBufferSize += StringSizeLength + userNameBytes.Length;
            }
            else
            {
                userNameBytes = null;
            }

            byte[] passwordBytes;
            if (packet.PasswordFlag)
            {
                string password = packet.Password;
                passwordBytes = EncodeStringInUtf8(password);
                payloadBufferSize += StringSizeLength + passwordBytes.Length;
            }
            else
            {
                passwordBytes = null;
            }

            // Fixed header
            byte[] protocolNameBytes = EncodeStringInUtf8(Util.ProtocolName);
            int variableHeaderBufferSize = StringSizeLength + protocolNameBytes.Length + 4;
            int variablePartSize = variableHeaderBufferSize + payloadBufferSize;
            int fixedHeaderBufferSize = 1 + MaxVariableLength;
            IByteBuffer buf = null;
            try
            {
                buf = bufferAllocator.Buffer(fixedHeaderBufferSize + variablePartSize);
                buf.WriteByte(CalculateFirstByteOfFixedHeader(packet));
                WriteVariableLengthInt(buf, variablePartSize);

                buf.WriteShort(protocolNameBytes.Length);
                buf.WriteBytes(protocolNameBytes);

                buf.WriteByte(Util.ProtocolLevel);
                buf.WriteByte(CalculateConnectFlagsByte(packet));
                buf.WriteShort(packet.KeepAlive);

                // Payload
                buf.WriteShort(clientIdBytes.Length);
                buf.WriteBytes(clientIdBytes, 0, clientIdBytes.Length);
                if (packet.WillFlag)
                {
                    buf.WriteShort(willTopicBytes.Length);
                    buf.WriteBytes(willTopicBytes, 0, willTopicBytes.Length);
                    buf.WriteShort(willMessage.ReadableBytes);
                    if (willMessage.IsReadable())
                    {
                        buf.WriteBytes(willMessage);
                    }
                    willMessage.Release();
                    willMessage = null;
                }
                if (packet.UsernameFlag)
                {
                    buf.WriteShort(userNameBytes.Length);
                    buf.WriteBytes(userNameBytes, 0, userNameBytes.Length);

                    if (packet.PasswordFlag)
                    {
                        buf.WriteShort(passwordBytes.Length);
                        buf.WriteBytes(passwordBytes, 0, passwordBytes.Length);
                    }
                }

                output.Add(buf);
                buf = null;
            }
            finally
            {
                buf?.SafeRelease();
                willMessage?.SafeRelease();
            }
        }

        static int CalculateConnectFlagsByte(ConnectPacket packet)
        {
            int flagByte = 0;
            if (packet.UsernameFlag)
            {
                flagByte |= 0x80;
            }
            if (packet.PasswordFlag)
            {
                flagByte |= 0x40;
            }
            if (packet.WillFlag)
            {
                flagByte |= 0x04;
                flagByte |= ((int)packet.WillQos & 0x03) << 3;
                if (packet.WillRetain)
                {
                    flagByte |= 0x20;
                }
            }
            if (packet.CleanSession)
            {
                flagByte |= 0x02;
            }
            return flagByte;
        }

        static void EncodeConnAckMessage(IByteBufferAllocator bufferAllocator, ConnAckPacket message, List<object> output)
        {
            IByteBuffer buffer = null;
            try
            {
                buffer = bufferAllocator.Buffer(4);
                buffer.WriteByte(CalculateFirstByteOfFixedHeader(message));
                buffer.WriteByte(2); // remaining length
                if (message.SessionPresent)
                {
                    buffer.WriteByte(1); // 7 reserved 0-bits and SP = 1
                }
                else
                {
                    buffer.WriteByte(0); // 7 reserved 0-bits and SP = 0
                }
                buffer.WriteByte((byte)message.ConnectReturnCode);


                output.Add(buffer);
                buffer = null;
            }
            finally
            {
                buffer?.SafeRelease();
            }
        }

        static void EncodePublishMessage(IByteBufferAllocator bufferAllocator, PublishPacket packet, List<object> output)
        {
            //IByteBuffer payload = packet.Payload ?? Unpooled.Empty;
            IByteBuffer payload;
            if (packet.Payload == null)
            {
                payload = Unpooled.Empty;
            }
            else
            {
                payload = Unpooled.Buffer();
                payload.WriteBytes(packet.Payload);
            }

            string topicName = packet.TopicName;
            Util.ValidateTopicName(topicName);
            byte[] topicNameBytes = EncodeStringInUtf8(topicName);

            int variableHeaderBufferSize = StringSizeLength + topicNameBytes.Length +
                (packet.Qos > MqttQos.AtMostOnce ? PacketIdLength : 0);
            int payloadBufferSize = payload.ReadableBytes;
            int variablePartSize = variableHeaderBufferSize + payloadBufferSize;
            int fixedHeaderBufferSize = 1 + MaxVariableLength;

            IByteBuffer buf = null;
            try
            {
                buf = bufferAllocator.Buffer(fixedHeaderBufferSize + variablePartSize);
                buf.WriteByte(CalculateFirstByteOfFixedHeader(packet));
                WriteVariableLengthInt(buf, variablePartSize);
                buf.WriteShort(topicNameBytes.Length);
                buf.WriteBytes(topicNameBytes);
                if (packet.Qos > MqttQos.AtMostOnce)
                {
                    buf.WriteShort(packet.PacketId);
                }

                output.Add(buf);
                buf = null;
            }
            finally
            {
                buf?.SafeRelease();
            }

            if (payload.IsReadable())
            {
                output.Add(payload.Retain());
            }
        }

        static void EncodePacketWithIdOnly(IByteBufferAllocator bufferAllocator, PacketWithId packet, List<object> output)
        {
            int msgId = packet.PacketId;

            const int VariableHeaderBufferSize = PacketIdLength; // variable part only has a packet id
            int fixedHeaderBufferSize = 1 + MaxVariableLength;
            IByteBuffer buffer = null;
            try
            {
                buffer = bufferAllocator.Buffer(fixedHeaderBufferSize + VariableHeaderBufferSize);
                buffer.WriteByte(CalculateFirstByteOfFixedHeader(packet));
                WriteVariableLengthInt(buffer, VariableHeaderBufferSize);
                buffer.WriteShort(msgId);

                output.Add(buffer);
                buffer = null;
            }
            finally
            {
                buffer?.SafeRelease();
            }
        }

        static void EncodeSubscribeMessage(IByteBufferAllocator bufferAllocator, SubscribePacket packet, List<object> output)
        {
            //const int VariableHeaderSize = PacketIdLength;
            //int payloadBufferSize = 0;

            //ThreadLocalObjectList encodedTopicFilters = ThreadLocalObjectList.NewInstance();

            //IByteBuffer buf = null;
            //try
            //{
            //    foreach (SubscriptionRequest topic in packet.Requests)
            //    {
            //        byte[] topicFilterBytes = EncodeStringInUtf8(topic.TopicFilter);
            //        payloadBufferSize += StringSizeLength + topicFilterBytes.Length + 1; // length, value, QoS
            //        encodedTopicFilters.Add(topicFilterBytes);
            //    }

            //    int variablePartSize = VariableHeaderSize + payloadBufferSize;
            //    int fixedHeaderBufferSize = 1 + MaxVariableLength;

            //    buf = bufferAllocator.Buffer(fixedHeaderBufferSize + variablePartSize);
            //    buf.WriteByte(CalculateFirstByteOfFixedHeader(packet));
            //    WriteVariableLengthInt(buf, variablePartSize);

            //    // Variable Header
            //    buf.WriteShort(packet.PacketId); // todo: review: validate?

            //    // Payload
            //    for (int i = 0; i < encodedTopicFilters.Count; i++)
            //    {
            //        var topicFilterBytes = (byte[])encodedTopicFilters[i];
            //        buf.WriteShort(topicFilterBytes.Length);
            //        buf.WriteBytes(topicFilterBytes, 0, topicFilterBytes.Length);
            //        buf.WriteByte((int)packet.Requests[i].QualityOfService);
            //    }

            //    output.Add(buf);
            //    buf = null;
            //}
            //finally
            //{
            //    buf?.SafeRelease();
            //    encodedTopicFilters.Return();
            //}
        }

        static void EncodeSubAckMessage(IByteBufferAllocator bufferAllocator, SubAckPacket message, List<object> output)
        {
            int payloadBufferSize = message.ReturnCodes.Count;
            int variablePartSize = PacketIdLength + payloadBufferSize;
            int fixedHeaderBufferSize = 1 + MaxVariableLength;
            IByteBuffer buf = null;
            try
            {
                buf = bufferAllocator.Buffer(fixedHeaderBufferSize + variablePartSize);
                buf.WriteByte(CalculateFirstByteOfFixedHeader(message));
                WriteVariableLengthInt(buf, variablePartSize);
                buf.WriteShort(message.PacketId);
                foreach (MqttQos qos in message.ReturnCodes)
                {
                    buf.WriteByte((byte)qos);
                }

                output.Add(buf);
                buf = null;

            }
            finally
            {
                buf?.SafeRelease();
            }
        }

        static void EncodeUnsubscribeMessage(IByteBufferAllocator bufferAllocator, UnsubscribePacket packet, List<object> output)
        {
            //const int VariableHeaderSize = 2;
            //int payloadBufferSize = 0;

            //ThreadLocalObjectList encodedTopicFilters = ThreadLocalObjectList.NewInstance();

            //IByteBuffer buf = null;
            //try
            //{
            //    foreach (string topic in packet.TopicFilters)
            //    {
            //        byte[] topicFilterBytes = EncodeStringInUtf8(topic);
            //        payloadBufferSize += StringSizeLength + topicFilterBytes.Length; // length, value
            //        encodedTopicFilters.Add(topicFilterBytes);
            //    }

            //    int variablePartSize = VariableHeaderSize + payloadBufferSize;
            //    int fixedHeaderBufferSize = 1 + MaxVariableLength;

            //    buf = bufferAllocator.Buffer(fixedHeaderBufferSize + variablePartSize);
            //    buf.WriteByte(CalculateFirstByteOfFixedHeader(packet));
            //    WriteVariableLengthInt(buf, variablePartSize);

            //    // Variable Header
            //    buf.WriteShort(packet.PacketId); // todo: review: validate?

            //    // Payload
            //    for (int i = 0; i < encodedTopicFilters.Count; i++)
            //    {
            //        var topicFilterBytes = (byte[])encodedTopicFilters[i];
            //        buf.WriteShort(topicFilterBytes.Length);
            //        buf.WriteBytes(topicFilterBytes, 0, topicFilterBytes.Length);
            //    }

            //    output.Add(buf);
            //    buf = null;
            //}
            //finally
            //{
            //    buf?.SafeRelease();
            //    encodedTopicFilters.Return();
            //}
        }

        static void EncodePacketWithFixedHeaderOnly(IByteBufferAllocator bufferAllocator, Packet packet, List<object> output)
        {
            IByteBuffer buffer = null;
            try
            {
                buffer = bufferAllocator.Buffer(2);
                buffer.WriteByte(CalculateFirstByteOfFixedHeader(packet));
                buffer.WriteByte(0);

                output.Add(buffer);
                buffer = null;
            }
            finally
            {
                buffer?.SafeRelease();
            }
        }

        static int CalculateFirstByteOfFixedHeader(Packet packet)
        {
            int ret = 0;
            ret |= (int)packet.PacketType << 4;
            if (packet.Dup)
            {
                ret |= 0x08;
            }
            ret |= (int)packet.Qos << 1;
            if (packet.Retain)
            {
                ret |= 0x01;
            }
            return ret;
        }

        static void WriteVariableLengthInt(IByteBuffer buffer, int value)
        {
            do
            {
                int digit = value % 128;
                value /= 128;
                if (value > 0)
                {
                    digit |= 0x80;
                }
                buffer.WriteByte(digit);
            }
            while (value > 0);
        }

        static byte[] EncodeStringInUtf8(string s)
        {
            // todo: validate against extra limitations per MQTT's UTF-8 string definition
            return Encoding.UTF8.GetBytes(s);
        }
    }
}