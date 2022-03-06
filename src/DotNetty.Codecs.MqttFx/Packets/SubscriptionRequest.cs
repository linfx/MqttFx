namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 订阅(Subscription request)
    /// 订阅包含主题筛选器和最大 QoS。订阅与单个会话相关联。一个会话可以包含多个订阅。会话中的每个订阅都有不同的主题筛选器。
    /// A Subscription comprises a Topic Filter and a maximum QoS. A Subscription is associated with a single Session. A Session can contain more than one Subscription. Each Subscription within a session has a different Topic Filter.
    /// </summary>
    public struct SubscriptionRequest
    {
        /// <summary>
        /// 主题筛选器（Topic Filter）
        /// 订阅中包含的表达式，用于指示对一个或多个主题的兴趣。主题过滤器可以包含通配符。
        /// An expression contained in a Subscription, to indicate an interest in one or more topics. A Topic Filter can include wildcard characters.
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
