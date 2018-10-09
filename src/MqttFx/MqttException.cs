using System;

namespace MqttFx
{
    public class MqttException : Exception
    {
        public MqttException() { }

        public MqttException(string message) : base(message) { }

        public MqttException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class MqttTimeoutException : Exception
    {
        public MqttTimeoutException(Exception ex) { }
    }
}