using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 有效载荷(Payload)
    /// The Payload contains the Application Message that is being published. 
    /// The content and format of the data is application specific. 
    /// The length of the payload can be calculated by subtracting the length of the variable header from the Remaining Length field that is in the Fixed Header. 
    /// It is valid for a PUBLISH Packet to contain a zero length payload.
    /// </summary>
    public record PublishPayload : Payload
    {
        byte[] body;

        public PublishPayload() { }

        public PublishPayload(byte[] payload) => body = payload;

        public override void Encode(IByteBuffer buffer, VariableHeader variableHeader)
        {
            if (body != null)
                buffer.WriteBytes(body);
        }

        public override void Decode(IByteBuffer buffer, VariableHeader variableHeader, ref int remainingLength)
        {
            body = buffer.ReadSliceArray(ref remainingLength);
        }

        public static implicit operator byte[](PublishPayload payload) => payload.body;

        public static implicit operator PublishPayload(byte[] payload) => new(payload);
    }
}
