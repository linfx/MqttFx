using Xunit;

namespace nMqtt.Test
{
    public class Class1
    {
        [Fact]
        public void TestConnAckMessage()
        {
            MqttClient client = new MqttClient();
            client.ConnectAsync().Wait();

        }
    }
}
