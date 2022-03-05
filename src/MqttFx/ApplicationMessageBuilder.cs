using System.Text;

namespace MqttFx
{
    public sealed class ApplicationMessageBuilder
    {
        string _topic;
        byte[] _payload;

        public ApplicationMessage Build()
        {
            var msg = new ApplicationMessage
            {
                Topic = _topic,
                Payload = _payload,
            };
            return msg;
        }
        public ApplicationMessageBuilder WithPayload(byte[] payload)
        {
            _payload = payload;
            return this;
        }

        public ApplicationMessageBuilder WithPayload(string payload)
        {
            if (payload == null)
            {
                _payload = null;
                return this;
            }

            _payload = string.IsNullOrEmpty(payload) ? null : Encoding.UTF8.GetBytes(payload);
            return this;
        }

        public ApplicationMessageBuilder WithTopic(string topic)
        {
            _topic = topic;
            return this;
        }
    }
}
