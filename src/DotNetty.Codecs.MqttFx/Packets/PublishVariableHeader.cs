using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 可变报头
    /// </summary>
    public class PublishVariableHeader : PacketIdVariableHeader
    {
        /// <summary>
        /// 主题名(UTF-8编码的字符串)(Topic Name)
        /// </summary>
        public string TopicName { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="fixedHeader"></param>
        public override void Encode(IByteBuffer buffer, FixedHeader fixedHeader)
        {
            buffer.WriteString(TopicName);
            base.Encode(buffer, fixedHeader);
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="fixedHeader"></param>
        public override void Decode(IByteBuffer buffer, ref FixedHeader fixedHeader)
        {
            TopicName = buffer.ReadString(ref fixedHeader.RemainingLength);
            base.Decode(buffer, ref fixedHeader);
        }
    }
}
