using Xunit;

namespace nMqtt.Test
{
    public class MqttClientTests
    {
        [Fact]
        public void TestConnect()
        {
            MqttClient client = new MqttClient();
            client.ConnectAsync().Wait();
        }
    }
}
