using System;

namespace MqttFx.Exceptions
{
    public class MqttTimeoutException : Exception
    {
        public MqttTimeoutException(Exception ex)
        {
        }
    }
}
