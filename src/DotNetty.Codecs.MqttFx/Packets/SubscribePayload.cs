using DotNetty.Buffers;
using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx.Packets;

/// <summary>
/// 有效载荷(SUBSCRIBE Packet payload)
/// If it chooses not to support topic filters that contain wildcard characters it MUST reject any Subscription request whose filter contains them [MQTT-3.8.3-2]. 
/// The payload of a SUBSCRIBE packet MUST contain at least one Topic Filter / QoS pair. A SUBSCRIBE packet with no payload is a protocol violation [MQTT-3.8.3-3].
/// </summary>
public class SubscribePayload : Payload
{
    /// <summary>
    /// 订阅请求(Subscription request)
    /// </summary>
    public IList<SubscriptionRequest> SubscriptionRequests { get; private set; }

    /// <summary>
    /// SUBSCRIBE Packet payload
    /// </summary>
    public SubscribePayload() { }

    /// <summary>
    /// SUBSCRIBE Packet payload
    /// </summary>
    /// <param name="requests">Subscription request</param>
    public SubscribePayload(params SubscriptionRequest[] requests) => SubscriptionRequests = requests;

    public override void Encode(IByteBuffer buffer, VariableHeader variableHeader)
    {
        foreach (var item in SubscriptionRequests)
        {
            buffer.WriteString(item.TopicFilter);
            buffer.WriteByte((byte)item.RequestedQos);
        }
    }

    public override void Decode(IByteBuffer buffer, VariableHeader variableHeader, ref int remainingLength)
    {
        SubscriptionRequests = new List<SubscriptionRequest>();
        while (remainingLength > 0)
        {
            string topicFilter = buffer.ReadString(ref remainingLength);
            MqttCodecUtil.ValidateTopicFilter(topicFilter);

            byte qos = buffer.ReadByte(ref remainingLength);
            if (qos > (byte)MqttQos.ExactlyOnce)
                throw new DecoderException($"The Server MUST treat a SUBSCRIBE packet as malformed and close the Network Connection if any of Reserved bits in the payload are non-zero, or QoS is not 0,1 or 2. [MQTT-3.8.3-4](Invalid QoS value: {qos}.)");

            SubscriptionRequest request;
            request.TopicFilter = topicFilter;
            request.RequestedQos = (MqttQos)qos;
            SubscriptionRequests.Add(request);
        }

        if (SubscriptionRequests.Count == 0)
            throw new DecoderException("The payload of a SUBSCRIBE packet MUST contain at least one Topic Filter / QoS pair. A SUBSCRIBE packet with no payload is a protocol violation. [MQTT-3.8.3-3]");
    }
}
