using System;

namespace nMqtt
{
    internal static class MqttUtils
    {
        public static short NewPacketId()
        {
            return (short)(Guid.NewGuid().GetHashCode() & ushort.MaxValue);
        }
    }
}
