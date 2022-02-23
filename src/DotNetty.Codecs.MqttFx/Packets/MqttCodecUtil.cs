namespace DotNetty.Codecs.MqttFx.Packets
{
    public static class MqttCodecUtil
    {
        public static void ValidateTopicFilter(string topicFilter)
        {
            int length = topicFilter.Length;
            if (length == 0)
                throw new DecoderException("[MQTT-4.7.3-1]");

            for (int i = 0; i < length; i++)
            {
                char c = topicFilter[i];
                switch (c)
                {
                    case '+':
                        if ((i > 0 && topicFilter[i - 1] != '/') || (i < length - 1 && topicFilter[i + 1] != '/'))
                        {
                            throw new DecoderException($"[MQTT-4.7.1-3]. Invalid topic filter: {topicFilter}");
                        }
                        break;
                    case '#':
                        if (i < length - 1 || (i > 0 && topicFilter[i - 1] != '/'))
                        {
                            throw new DecoderException($"[MQTT-4.7.1-2]. Invalid topic filter: {topicFilter}");
                        }
                        break;
                }
            }
        }
    }
}
