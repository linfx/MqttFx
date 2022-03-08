using System;
using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx.Packets
{
    public static class MqttCodecUtil
    {
        public static IDictionary<Type, PacketType> PacketTypes = new Dictionary<Type, PacketType>
        {
            { typeof(ConnectPacket), PacketType.CONNECT },
            { typeof(ConnAckPacket), PacketType.CONNACK },
            { typeof(PublishPacket), PacketType.PUBLISH },
            { typeof(PubAckPacket), PacketType.PUBACK },
            { typeof(PubRecPacket), PacketType.PUBREC },
            { typeof(PubRelPacket), PacketType.PUBREL },
            { typeof(PubCompPacket), PacketType.PUBCOMP },
            { typeof(SubscribePacket), PacketType.SUBSCRIBE },
            { typeof(SubAckPacket), PacketType.SUBACK },
            { typeof(UnsubscribePacket), PacketType.UNSUBSCRIBE },
            { typeof(UnsubAckPacket), PacketType.UNSUBACK },
            { typeof(PingReqPacket), PacketType.PINGREQ },
            { typeof(PingRespPacket), PacketType.PINGRESP },
            { typeof(DisconnectPacket), PacketType.DISCONNECT },
        };

        public static void ValidateTopicFilter(string topicFilter)
        {
            int length = topicFilter.Length;
            if (length == 0)
                throw new DecoderException("All Topic Names and Topic Filters MUST be at least one character long. [MQTT-4.7.3-1]");

            for (int i = 0; i < length; i++)
            {
                char c = topicFilter[i];
                switch (c)
                {
                    case '+':
                        if ((i > 0 && topicFilter[i - 1] != '/') || (i < length - 1 && topicFilter[i + 1] != '/'))
                            throw new DecoderException($"[MQTT-4.7.1-3]. Invalid topic filter: {topicFilter}");

                        break;
                    case '#':
                        if (i < length - 1 || (i > 0 && topicFilter[i - 1] != '/'))
                            throw new DecoderException($"[MQTT-4.7.1-2]. Invalid topic filter: {topicFilter}");

                        break;
                }
            }
        }
    }

    public static class PublishPacketFixedHeaderExtensions
    {
        public static bool GetDup(this FixedHeader fixedHeader)
        {
            return (fixedHeader.Flags & 0x08) == 0x08;
        }

        public static MqttQos GetQos(this FixedHeader fixedHeader)
        {
            return (MqttQos)((fixedHeader.Flags & 0x06) >> 1);
        }

        public static bool GetRetain(this FixedHeader fixedHeader)
        {
            return (fixedHeader.Flags & 0x01) > 0;
        }

        public static void SetDup(this FixedHeader fixedHeader, bool dup = false)
        {
            fixedHeader.Flags |= dup.ToByte() << 3;
        }

        public static void SetQos(this FixedHeader fixedHeader, MqttQos qos)
        {
            fixedHeader.Flags |= (byte)qos << 1;
        }

        public static void SetRetain(this FixedHeader fixedHeader, bool retain = false)
        {
            fixedHeader.Flags |= retain.ToByte();
        }
    }
}
