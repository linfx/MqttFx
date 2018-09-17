namespace nMqtt
{
    public class MqttClientOptionsBuilder
    {
        private readonly MqttClientOptions _options = new MqttClientOptions();

        public MqttClientOptionsBuilder WithCleanSession(bool value = true)
        {
            _options.CleanSession = value;
            return this;
        }

        public MqttClientOptionsBuilder WithClientId(string clientId)
        {
            _options.ClientId = clientId;
            return this;
        }

        public MqttClientOptionsBuilder WithCredentials(string username, string password = default)
        {
            _options.Credentials = new MqttClientCredentials
            {
                Username = username,
                Password = password
            };
            return this;
        }

        public MqttClientOptionsBuilder WithTcpServer(string server, int port = 1883)
        {
            _options.Server = server;
            _options.Port = port;
            return this;
        }

        public MqttClientOptions Build()
        {
            return _options;
        }
    }
}
