using DotNetty.Buffers;
using System;

namespace DotNetty.Codecs.MqttFx.Packets;

/// <summary>
/// 有效载荷(SUBACK Packet payload)
/// The payload contains a list of return codes. Each return code corresponds to a Topic Filter in the SUBSCRIBE Packet being acknowledged. The order of return codes in the SUBACK Packet MUST match the order of Topic Filters in the SUBSCRIBE Packet [MQTT-3.9.3-1].
/// </summary>
public class SubAckPayload : Payload
{
    /// <summary>
    /// 返回代码
    /// </summary>
    public MqttQos[] ReturnCodes { get; private set; }

    public SubAckPayload() { }

    public SubAckPayload(params MqttQos[] returnCodes)
    {
        ReturnCodes = returnCodes;
    }

    public override void Encode(IByteBuffer buffer, VariableHeader variableHeader)
    {
        foreach (var qos in ReturnCodes)
        {
            buffer.WriteByte((byte)qos);
        }
    }

    public override void Decode(IByteBuffer buffer, VariableHeader variableHeader, ref int remainingLength)
    {
        Span<MqttQos> buf = stackalloc MqttQos[remainingLength];
        for (int i = 0; i < buf.Length; i++)
        {
            var returnCode = (MqttQos)buffer.ReadByte(ref remainingLength);
            if (returnCode > MqttQos.ExactlyOnce && returnCode != MqttQos.Failure)
            {
                throw new DecoderException($"SUBACK return codes other than 0x00, 0x01, 0x02 and 0x80 are reserved and MUST NOT be used. [MQTT-3.9.3-2](Invalid return code: {returnCode}).");
            }
            buf[i] = returnCode;

        }
        ReturnCodes = buf.ToArray();
    }
}
