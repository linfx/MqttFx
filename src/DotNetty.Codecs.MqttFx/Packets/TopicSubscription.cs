namespace DotNetty.Codecs.MqttFx.Packets
{
    public struct TopicSubscription
    {
        /// <summary>
        /// Topic Filter
        /// </summary>
        public string TopicName;

        /// <summary>
        /// Requested QoS
        /// </summary>
        public MqttQos Qos;
    }
}
