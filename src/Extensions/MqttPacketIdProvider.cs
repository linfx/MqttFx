namespace MqttFx.Extensions
{
    public class MqttPacketIdProvider
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
                _value++;

                if (_value == 0)
                    _value = 1;

                return _value;
            }
        }
    }
}
