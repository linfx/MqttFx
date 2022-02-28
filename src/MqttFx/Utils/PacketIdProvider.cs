namespace MqttFx.Utils
{
    internal class PacketIdProvider
    {
        private readonly object _syncRoot = new();
        private ushort _value;

        public void Reset()
        {
            lock (_syncRoot)
                _value = 0;
        }

        public ushort NewPacketId()
        {
            lock (_syncRoot)
            {
                if (_value == ushort.MaxValue)
                    _value = 0;

                _value++;

                return _value;
            }
        }
    }
}