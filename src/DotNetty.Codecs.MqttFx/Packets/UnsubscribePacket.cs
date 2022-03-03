﻿using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 取消订阅(UNSUBSCRIBE – Unsubscribe from topics)
    /// </summary>
    public sealed class UnsubscribePacket : PacketWithId
    {
        public UnsubscribePacket()
            : this(new PacketIdVariableHeader(), new UnsubscribePayload(new List<string>())) { }

        public UnsubscribePacket(ushort packetId, params string[] topics)
            : this(new PacketIdVariableHeader(packetId), new UnsubscribePayload(topics)) { }

        public UnsubscribePacket(PacketIdVariableHeader variableHeader, UnsubscribePayload payload)
            : base(variableHeader, payload) { }

        public IList<string> Topics
        {
            get { return ((UnsubscribePayload)Payload).Topics; }
            set { ((UnsubscribePayload)Payload).Topics = value; }
        }
    }
}
