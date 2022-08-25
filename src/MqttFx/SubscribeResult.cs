using DotNetty.Codecs.MqttFx.Packets;
using System.Collections.Generic;

namespace MqttFx;

public class SubscribeResult
{
    public IReadOnlyCollection<SubscribeResultItem> Items { get; internal set; }

    public SubscribeResult(IReadOnlyCollection<SubscribeResultItem> items)
    {
        Items = items;
    }
}

public struct SubscribeResultItem
{
    public string TopicFilter;

    public MqttQos ResultCode { get; internal set; }
}
