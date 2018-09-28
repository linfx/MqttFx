using System;
using System.Collections.Generic;
using MqttFx.Packets;
using MqttFx.Protocol;

namespace MqttFx.Extensions
{
    public class MqttPacketTypeProvider
    {
        static Dictionary<Type, PacketType> _cache = new Dictionary<Type, PacketType>
        {
            { typeof(ConnectPacket), PacketType.CONNECT },
            { typeof(ConnAckPacket), PacketType.CONNACK },
            { typeof(DisconnectPacket), PacketType.DISCONNECT },

            { typeof(PingReqPacket), PacketType.PINGREQ },
            { typeof(PingRespPacket), PacketType.PINGRESP },

            { typeof(PublishPacket), PacketType.PUBLISH },
            { typeof(PubAckPacket), PacketType.PUBACK },
            { typeof(PubRecPacket), PacketType.PUBREC },
            { typeof(PubRelPacket), PacketType.PUBREL },
            { typeof(PubCompPacket), PacketType.PUBCOMP },

            { typeof(SubscribePacket), PacketType.PUBREL },
            { typeof(SubAckPacket), PacketType.PUBCOMP },

            { typeof(UnsubscribePacket), PacketType.PUBREL },
            { typeof(UnsubscribeAckMessage), PacketType.PUBCOMP },
        };

        public static PacketType GetPacketType(Type type)
        {
            if (!_cache.TryGetValue(type, out PacketType packetType))
            {
                throw new Exception("PacketType Error");
            }

            return packetType;
        }
    }
}
