using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 有效载荷(Payload)
    /// </summary>
    public abstract class Payload
    {
        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="buffer"></param>
        public virtual void Encode(IByteBuffer buffer, VariableHeader variableHeader) { }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="buffer"></param>
        public virtual void Decode(IByteBuffer buffer, FixedHeader fixedHeader, VariableHeader variableHeader) { }
    }
}
