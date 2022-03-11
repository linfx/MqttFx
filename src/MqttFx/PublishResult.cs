namespace MqttFx;

public class PublishResult
{
    /// <summary>
    /// Gets the packet identifier which was used for this publish.
    /// </summary>
    public ushort PacketId { get; set; }

    public PublishResult() { }

    public PublishResult(ushort packetId)
    {
        PacketId = packetId;
    }
}
