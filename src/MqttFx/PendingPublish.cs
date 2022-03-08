using System.Threading.Tasks;

namespace MqttFx
{
    class PendingPublish
    {
        public TaskCompletionSource<PublishResult> Future { get; set; } = new();
    }
}
