namespace MqttFx.Internal
{
    internal class PacketIdProvider
    {
        private readonly object _syncRoot = new object();
        private ushort _value;

        public void Reset()
        {
            lock (_syncRoot)
                _value = 0;
        }

        public ushort GetNewPacketId()
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