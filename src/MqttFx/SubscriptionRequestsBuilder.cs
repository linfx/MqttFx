using DotNetty.Codecs.MqttFx.Packets;
using System;

namespace MqttFx
{
    public class SubscriptionRequestsBuilder
    {
        SubscriptionRequests _requests = new();

        public SubscriptionRequestsBuilder WithTopicFilter(TopicFilter topicFilter)
        {
            if (topicFilter == null)
                throw new ArgumentNullException(nameof(topicFilter));

            SubscriptionRequest request;
            request.TopicFilter = topicFilter.Topic;
            request.RequestedQos = topicFilter.Qos;
            _requests.Requests.Add(request);

            return this;
        }

        public SubscriptionRequestsBuilder WithTopicFilter(Action<TopicFilterBuilder> topicFilterBuilder)
        {
            if (topicFilterBuilder == null) 
                throw new ArgumentNullException(nameof(topicFilterBuilder));

            var internalTopicFilterBuilder = new TopicFilterBuilder();
            topicFilterBuilder(internalTopicFilterBuilder);

            return WithTopicFilter(internalTopicFilterBuilder);
        }

        public SubscriptionRequestsBuilder WithTopicFilter(TopicFilterBuilder topicFilterBuilder)
        {
            if (topicFilterBuilder == null)
                throw new ArgumentNullException(nameof(topicFilterBuilder));

            return WithTopicFilter(topicFilterBuilder.Build());
        }

        public SubscriptionRequests Build()
        {
            return _requests;
        }
    }
}
