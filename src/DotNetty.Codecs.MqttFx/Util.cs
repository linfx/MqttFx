namespace DotNetty.Codecs.MqttFx
{
    public static class Util
    {
        public const string ProtocolName = "MQTT";
        public const int ProtocolLevel = 4;
        private static readonly char[] TopicWildcards = { '#', '+' };

        public static void ValidateTopicName(string topicName)
        {
            if (topicName.Length == 0)
            {
                throw new DecoderException("[MQTT-4.7.3-1]");
            }

            if (topicName.IndexOfAny(TopicWildcards) > 0)
            {
                throw new DecoderException($"Invalid PUBLISH topic name: {topicName}");
            }
        }

        public static void ValidatePacketId(int packetId)
        {
            if (packetId < 1)
            {
                throw new DecoderException("Invalid packet identifier: " + packetId);
            }
        }

        public static void ValidateClientId(string clientId)
        {
            if (clientId == null)
            {
                throw new DecoderException("Client identifier is required.");
            }
        }
    }
}
