using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx.Packets;

/// <summary>
/// 订阅报文(SUBSCRIBE - Subscribe to topics)
/// The SUBSCRIBE Packet is sent from the Client to the Server to create one or more Subscriptions.
/// Each Subscription registers a Client’s interest in one or more Topics. 
/// The Server sends PUBLISH Packets to the Client in order to forward Application Messages that were published to Topics that match these Subscriptions. 
/// The SUBSCRIBE Packet also specifies (for each Subscription) the maximum QoS with which the Server can send Application Messages to the Client.
/// </summary>
public sealed record class SubscribePacket : PacketWithId
{
    /// <summary>
    /// SUBSCRIBE - Subscribe to topics
    /// </summary>
    public SubscribePacket()
        : this(new PacketIdVariableHeader(), new SubscribePayload()) { }

    /// <summary>
    /// SUBSCRIBE - Subscribe to topics
    /// </summary>
    /// <param name="packetId">Packet Identifier</param>
    /// <param name="requests">Subscription request</param>
    public SubscribePacket(ushort packetId, params SubscriptionRequest[] requests)
        : this(new PacketIdVariableHeader(packetId), new SubscribePayload(requests)) { }

    /// <summary>
    /// SUBSCRIBE - Subscribe to topics
    /// </summary>
    /// <param name="variableHeader"></param>
    /// <param name="payload"></param>
    public SubscribePacket(PacketIdVariableHeader variableHeader, SubscribePayload payload)
        : base(variableHeader, payload) { }

    /// <summary>
    /// SUBSCRIBE - Subscribe to topics
    /// </summary>
    /// <param name="fixedHeader"></param>
    /// <param name="variableHeader"></param>
    /// <param name="payload"></param>
    public SubscribePacket(FixedHeader fixedHeader, PacketIdVariableHeader variableHeader, SubscribePayload payload)
        : base(fixedHeader, variableHeader, payload) { }

    public IList<SubscriptionRequest> SubscriptionRequests
    {
        get => ((SubscribePayload)Payload).SubscriptionRequests;
    }

    /// <summary>
    /// 订阅主题
    /// </summary>
    /// <param name="topicFilter">Topic Name</param>
    /// <param name="requestedQos">Requested QoS</param>
    public void AddSubscriptionRequest(string topicFilter, MqttQos requestedQos)
    {
        SubscriptionRequest request;
        request.TopicFilter = topicFilter;
        request.RequestedQos = requestedQos;
        SubscriptionRequests.Add(request);
    }
}
