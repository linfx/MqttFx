using DotNetty.Buffers;
using DotNetty.Common.Utilities;

namespace DotNetty.Codecs.MqttFx.Packets;

/// <summary>
/// MQTT 控制数据包(MQTT Control Packet)
/// 通过网络连接发送的信息包。MQTT 规范定义了 14 种不同类型的控制数据包。
/// 
/// </summary>
public abstract record class Packet
{
    /// <summary>
    /// 固定报头(Fixed header)
    /// </summary>
    public FixedHeader FixedHeader;

    /// <summary>
    /// 可变报头(Variable header)
    /// </summary>
    public VariableHeader VariableHeader;

    /// <summary>
    /// 有效载荷(Payload)
    /// </summary>
    public Payload Payload;

    /// <summary>
    /// 报文抽象类(MQTT Control Packet)
    /// </summary>
    protected Packet()
    {
        FixedHeader.PacketType = MqttCodecUtil.PacketTypes[GetType()];
    }

    /// <summary>
    /// 报文抽象类(MQTT Control Packet)
    /// </summary>
    /// <param name="variableHeader">可变报头(Variable header)</param>
    protected Packet(VariableHeader variableHeader)
        : this(variableHeader, default) { }

    /// <summary>
    /// 报文抽象类(MQTT Control Packet)
    /// </summary>
    /// <param name="variableHeader">可变报头(Variable header)</param>
    /// <param name="payload">有效载荷(Payload)</param>
    protected Packet(VariableHeader variableHeader, Payload payload)
        : this()
    {
        VariableHeader = variableHeader;
        Payload = payload;
    }

    /// <summary>
    /// 报文抽象类(MQTT Control Packet)
    /// </summary>
    /// <param name="fixedHeader">固定报头(Fixed header)</param>
    /// <param name="variableHeader">可变报头(Variable header)</param>
    /// <param name="payload">有效载荷(Payload)</param>
    protected Packet(FixedHeader fixedHeader, VariableHeader variableHeader, Payload payload)
        : this()
    {
        FixedHeader = fixedHeader;
        VariableHeader = variableHeader;
        Payload = payload;
    }

    /// <summary>
    /// 编码
    /// </summary>
    /// <param name="buffer"></param>
    public virtual void Encode(IByteBuffer buffer)
    {
        var buf = Unpooled.Buffer();
        try
        {
            VariableHeader?.Encode(buf, FixedHeader);
            Payload?.Encode(buf, VariableHeader);
            FixedHeader.Encode(buffer, buf.ReadableBytes);

            buffer.WriteBytes(buf);
        }
        finally
        {
            buf?.SafeRelease();
        }
    }

    /// <summary>
    /// 解码
    /// </summary>
    /// <param name="buffer"></param>
    public virtual void Decode(IByteBuffer buffer)
    {
        VariableHeader?.Decode(buffer, ref FixedHeader);
        Payload?.Decode(buffer, VariableHeader, ref FixedHeader.RemainingLength);
    }
}
