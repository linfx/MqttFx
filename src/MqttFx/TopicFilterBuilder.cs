using DotNetty.Codecs.MqttFx.Packets;

namespace MqttFx
{
    public sealed class TopicFilterBuilder
    {
        /// <summary>
        /// The MQTT topic.
        /// In MQTT, the word topic refers to an UTF-8 string that the broker uses to filter messages for each connected client.
        /// The topic consists of one or more topic levels. Each topic level is separated by a forward slash (topic level separator).
        /// </summary>
        string _topic;

        /// <summary>
        /// The quality of service level.
        /// The Quality of Service (QoS) level is an agreement between the sender of a message and the receiver of a message that defines the guarantee of delivery for a specific message.
        /// There are 3 QoS levels in MQTT:
        /// - At most once  (0): Message gets delivered no time, once or multiple times.
        /// - At least once (1): Message gets delivered at least once (one time or more often).
        /// - Exactly once  (2): Message gets delivered exactly once (It's ensured that the message only comes once).
        /// </summary>
        MqttQos _qos = MqttQos.AT_MOST_ONCE;

        public TopicFilterBuilder WithTopic(string topic)
        {
            _topic = topic;
            return this;
        }

        public TopicFilterBuilder WithQos(MqttQos qos)
        {
            _qos = qos;
            return this;
        }

        public TopicFilterBuilder WithAtLeastOnceQoS()
        {
            _qos = MqttQos.AT_LEAST_ONCE;
            return this;
        }

        public TopicFilterBuilder WithAtMostOnceQoS()
        {
            _qos = MqttQos.AT_MOST_ONCE;
            return this;
        }

        public TopicFilterBuilder WithExactlyOnceQoS()
        {
            _qos = MqttQos.EXACTLY_ONCE;
            return this;
        }

        public TopicFilter Build()
        {
            if (string.IsNullOrEmpty(_topic))
                throw new MqttException("Topic is not set.");

            return new TopicFilter
            {
                Topic = _topic,
                Qos = _qos,
            };
        }
    }
}
