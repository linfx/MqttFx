using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 可变报头(Variable header)
    /// </summary>
    public abstract class VariableHeader
    {
        public virtual void Encode(IByteBuffer buffer) { }

        public virtual void Encode(IByteBuffer buffer, FixedHeader fixedHeader) => Encode(buffer);

        public virtual void Decode(IByteBuffer buffer, ref FixedHeader fixedHeader) { }
    }
}