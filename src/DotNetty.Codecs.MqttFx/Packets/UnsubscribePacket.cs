using System.Collections.Generic;
using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 取消订阅
    /// </summary>
    public sealed class UnsubscribePacket : PacketWithId
    {
        private readonly List<string> _topics = new List<string>();

        public UnsubscribePacket() 
            : base(PacketType.UNSUBSCRIBE)
        {
        }

        public void AddRange(params string[] topics)
        {
            _topics.AddRange(topics);
        }

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
            }
            finally
            {
                buf?.Release();
            }
        }
    }

    /// <summary>
    /// 取消订阅回执
    /// </summary>
    public sealed class UnsubAckPacket : PacketWithId
    {
        public UnsubAckPacket()
            : base(PacketType.UNSUBACK)
        {
        }
    }
}
