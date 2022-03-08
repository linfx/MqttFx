using DotNetty.Codecs.MqttFx.Packets;
using System.Threading.Tasks;

namespace MqttFx
{
    class PendingSubscription
    {
        public TaskCompletionSource<SubscribeResult> Future { get; set; } = new();

        public SubscribePacket SubscribePacket { get; set; }

        public PendingSubscription(SubscribePacket packet)
        {
            SubscribePacket = packet;
        }
    }
}
