namespace MqttFx
{
    public class UnSubscribeResult
    {
        public ushort PacketId { get; set; }

        public UnSubscribeResult(ushort packetId)
        {
            PacketId = packetId;
        }
    }
}
