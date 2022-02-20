using System.Collections.Generic;
using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 取消订阅
    /// </summary>
    public sealed class UnsubscribePacket : PacketWithIdentifier
    {
        private readonly List<string> _topics = new List<string>();

        public UnsubscribePacket() 
            : base(PacketType.UNSUBSCRIBE) { }

        public void AddRange(params string[] topics) => _topics.AddRange(topics);

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="buffer"></param>
        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                buf.WriteShort(VariableHeader.PacketIdentifier);

                foreach (var item in _topics)
                {
                    buf.WriteString(item);
                }

                FixedHeader.RemainingLength = buf.ReadableBytes;
                FixedHeader.Encode(buffer);
                buffer.WriteBytes(buf);
            }
            finally
            {
                buf?.Release();
            }
        }
    }
}
