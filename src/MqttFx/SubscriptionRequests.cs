using DotNetty.Codecs.MqttFx.Packets;
using System.Collections.Generic;

namespace MqttFx
{
    public class SubscriptionRequests
    {
        /// <summary>
        /// Gets or sets a list of topic filters the client wants to subscribe to.
        /// Topic filters can include regular topics or wild cards.
        /// </summary>
        public List<SubscriptionRequest> Requests { get; set; } = new List<SubscriptionRequest>();
    }
}
