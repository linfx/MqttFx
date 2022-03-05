namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// Subscription request
    /// </summary>
    public struct SubscriptionRequest
    {
        /// <summary>
        /// Topic Filter
        /// The Topic Filters in a SUBSCRIBE packet payload MUST be UTF-8 encoded strings as defined in Section 1.5.3 [MQTT-3.8.3-1]. 
        /// </summary>
        public string TopicFilter;

        /// <summary>
        /// Requested QoS
        /// The upper 6 bits of the Requested QoS byte are not used in the current version of the protocol. They are reserved for future use. 
        /// </summary>
        public MqttQos RequestedQos;
    }
}
