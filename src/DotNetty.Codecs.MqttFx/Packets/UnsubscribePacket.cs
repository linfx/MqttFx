using System.Collections.Generic;
using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 取消订阅
    /// </summary>
    public sealed class UnsubscribePacket : PacketWithId
    {
        public UnsubscribePacket() 
            : base(PacketType.UNSUBSCRIBE)
        {
        }

        List<string> _topics = new List<string>();

        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                buf.WriteShort(PacketId);

                foreach (var item in _topics)
                {
                    buf.WriteString(item);
                }

                FixedHeader.RemaingLength = buf.ReadableBytes;
                FixedHeader.WriteTo(buffer);
                buffer.WriteBytes(buf);
                buf = null;
            }
            finally
            {
                buf?.Release();
            }
        }

        public void AddRange(params string[] topics)
        {
            _topics.AddRange(topics);
        }
    }

    /// <summary>
    /// 取消订阅回执
    /// </summary>
    public sealed class UnsubAckPacket : Packet
    {
        public UnsubAckPacket()
            : base(PacketType.UNSUBACK)
        {
        }
    }
}
