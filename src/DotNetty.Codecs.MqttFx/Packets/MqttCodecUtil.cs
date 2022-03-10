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
        public static MqttQos GetQos(this FixedHeader fixedHeader)
        {
            return (MqttQos)((fixedHeader.Flags & 0x06) >> 1);
        }

        public static bool GetDup(this PublishPacket packet)
        {
            return (packet.FixedHeader.Flags & 0x08) == 0x08;
        }

        public static MqttQos GetQos(this PublishPacket packet)
        {
            return (MqttQos)((packet.FixedHeader.Flags & 0x06) >> 1);
        }

        public static bool GetRetain(this PublishPacket packet)
        {
            return (packet.FixedHeader.Flags & 0x01) > 0;
        }

        public static void SetQos(this PublishPacket packet, MqttQos qos)
        {
            packet.FixedHeader.Flags |= (byte)qos << 1;
        }

        public static void SetDup(this PublishPacket packet, bool dup = false)
        {
            packet.FixedHeader.Flags |= dup.ToByte() << 3;
        }

        public static void SetRetain(this PublishPacket packet, bool retain = false)
        {
            packet.FixedHeader.Flags |= retain.ToByte();
        }
    }
}
