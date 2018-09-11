using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using nMqtt.Protocol;

namespace nMqtt.Packets
{
    /// <summary>
    /// 订阅主题
    /// </summary>
    [PacketType(PacketType.SUBSCRIBE)]
    public sealed class SubscribePacket : PacketWithId
    {
        /// <summary>
        /// 主题列表
        /// </summary>
        IList<TopicQos> Topics = new List<TopicQos>();

        public void Add(string topic, MqttQos qos)
        {
            Topics.Add(new TopicQos
            {
                Topic = topic,
                Qos = qos,
            });
        }

        struct TopicQos
        {
            public string Topic { get; set; }
            public MqttQos Qos { get; set; }
        }

        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                buf.WriteUnsignedShort(PacketId);

                foreach (var item in Topics)
                {
                    buf.WriteString(item.Topic);
                    buf.WriteByte((byte)item.Qos);
                }

                FixedHeader.RemaingLength = buf.ReadableBytes;
                FixedHeader.WriteTo(buffer);
                buffer.WriteBytes(buf);
                buf = null;
            }
            finally
            {
                buf?.Release();
            }
        }
    }

    /// <summary>
    /// 订阅回执
    /// </summary>
    [PacketType(PacketType.SUBACK)]
    public class SubAckPacket : PacketWithId
    {
        public IReadOnlyList<SubscribeReturnCode> ReturnCodes { get; set; }

        public override void Decode(IByteBuffer buffer)
        {
            base.Decode(buffer);

            var returnCodes = new SubscribeReturnCode[RemaingLength];
            for (int i = 0; i < RemaingLength; i++)
            {
                var returnCode = (SubscribeReturnCode)buffer.ReadByte();
                if (returnCode > SubscribeReturnCode.SuccessMaximumQoS2 && returnCode != SubscribeReturnCode.Failure)
                {
                    throw new DecoderException($"[MQTT-3.9.3-2]. Invalid return code: {returnCode}");
                }
                returnCodes[i] = returnCode;
            }
            ReturnCodes = returnCodes;
            FixedHeader.RemaingLength = 0;
        }
    }
}