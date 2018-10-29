using System.Collections.Generic;
using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 订阅主题
    /// </summary>
    public sealed class SubscribePacket : PacketWithId
    {
        public SubscribePacket() 
            : base(PacketType.SUBSCRIBE)
        {
        }

        /// <summary>
        /// 主题列表
        /// </summary>
        List<SubscriptionRequest> _subscribeTopics = new List<SubscriptionRequest>();

        public void Add(string topic, MqttQos qos)
        {
            _subscribeTopics.Add(new SubscriptionRequest(topic, qos));
        }

        public void Add(params SubscriptionRequest[] requests)
        {
            _subscribeTopics.AddRange(requests);
        }

        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                buf.WriteUnsignedShort(PacketId);

                foreach (var item in _subscribeTopics)
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
    public class SubAckPacket : PacketWithId
    {
        public SubAckPacket()
            : base(PacketType.SUBACK)
        {
        }

        /// <summary>
        /// 返回代码
        /// </summary>
        public IReadOnlyList<MqttQos> ReturnCodes { get; set; }

        public override void Decode(IByteBuffer buffer)
        {
            base.Decode(buffer);

            var returnCodes = new MqttQos[RemaingLength];
            for (int i = 0; i < RemaingLength; i++)
            {
                var returnCode = (MqttQos)buffer.ReadByte();
                if (returnCode > MqttQos.ExactlyOnce && returnCode != MqttQos.Failure)
                {
                    throw new DecoderException($"[MQTT-3.9.3-2]. Invalid return code: {returnCode}");
                }
                returnCodes[i] = returnCode;
            }
            ReturnCodes = returnCodes;
            FixedHeader.RemaingLength = 0;
        }
    }

    public class SubscriptionRequest
    {
        public SubscriptionRequest(string topic, MqttQos qos)
        {
            Topic = topic;
            Qos = qos;
        }

        public string Topic { get; set; }
        public MqttQos Qos { get; set; }
    }
}