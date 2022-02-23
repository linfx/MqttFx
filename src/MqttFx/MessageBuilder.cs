using System.Text;

namespace MqttFx
{
    public sealed class MessageBuilder
    {
        string _topic;
        byte[] _payload;

        public Message Build()
        {
            var msg = new Message
            {
                Topic = _topic,
                Payload = _payload,
            };
            return msg;
        }
        public MessageBuilder WithPayload(byte[] payload)
        {
            _payload = payload;
            return this;
        }

        public MessageBuilder WithPayload(string payload)
        {
            if (payload == null)
            {
                _payload = null;
                return this;
            }

            _payload = string.IsNullOrEmpty(payload) ? null : Encoding.UTF8.GetBytes(payload);
            return this;
        }

        public MessageBuilder WithTopic(string topic)
        {
            _topic = topic;
            return this;
        }
    }
}
