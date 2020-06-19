using DotNetty.Buffers;
using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 订阅回执
    /// </summary>
    public class SubAckPacket : PacketWithIdentifier
    {
        public SubAckPacket()
            : base(PacketType.SUBACK) { }

        /// <summary>
        /// 返回代码
        /// </summary>
        public IReadOnlyList<MqttQos> ReturnCodes { get; set; }

        public override void Decode(IByteBuffer buffer)
        {
            base.Decode(buffer);
            var returnCodes = new MqttQos[FixedHeader.RemaingLength];
            for (int i = 0; i < FixedHeader.RemaingLength; i++)
            {
                var returnCode = (MqttQos)buffer.ReadByte();
                if (returnCode > MqttQos.ExactlyOnce)
                {
                    throw new DecoderException($"[MQTT-3.9.3-2]. Invalid return code: {returnCode}");
                }
                returnCodes[i] = returnCode;
            }
            ReturnCodes = returnCodes;
        }
    }
}