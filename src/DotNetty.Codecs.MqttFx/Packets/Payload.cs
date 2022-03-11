using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets;

/// <summary>
/// 有效载荷(Payload)
/// </summary>
public abstract record Payload
{
    /// <summary>
    /// 编码
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="variableHeader">可变报头</param>
    public virtual void Encode(IByteBuffer buffer, VariableHeader variableHeader) { }

    /// <summary>
    /// 解码
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="variableHeader">可变报头</param>
    public virtual void Decode(IByteBuffer buffer, VariableHeader variableHeader, ref int remainingLength) { }
}
