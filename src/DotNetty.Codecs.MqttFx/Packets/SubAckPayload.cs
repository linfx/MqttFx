using DotNetty.Buffers;
using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 有效载荷(SUBACK Packet payload)
    /// </summary>
    public class SubAckPayload : Payload
    {
        /// <summary>
        /// 返回代码
        /// </summary>
        public IList<MqttQos> ReturnCodes { get; set; }

        public override void Encode(IByteBuffer buffer, VariableHeader variableHeader)
        {
            foreach (var qos in ReturnCodes)
            {
                buffer.WriteByte((byte)qos);
            }
        }

        public override void Decode(IByteBuffer buffer, VariableHeader variableHeader, ref int remainingLength)
        {
            var returnCodes = new MqttQos[remainingLength];
            for (int i = 0; i < returnCodes.Length; i++)
            {
                var returnCode = (MqttQos)buffer.ReadByte(ref remainingLength);
                if (returnCode > MqttQos.EXACTLY_ONCE && returnCode != MqttQos.FAILURE)
                {
                    throw new DecoderException($"[MQTT-3.9.3-2]. Invalid return code: {returnCode}");
                }
                returnCodes[i] = returnCode;
            }
            ReturnCodes = returnCodes;
        }
    }
}
