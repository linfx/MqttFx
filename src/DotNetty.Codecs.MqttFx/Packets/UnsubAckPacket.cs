﻿namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 取消订阅回执
    /// </summary>
    public sealed class UnsubAckPacket : PacketWithIdentifier
    {
        public UnsubAckPacket()
            : base(PacketType.UNSUBACK) { }
    }
}
