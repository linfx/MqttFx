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
    public class PublishPayload : Payload
    {
        /// <summary>
        /// 有效载荷
        /// </summary>
        public byte[] Payload { get; set; }

        public PublishPayload() { }

        public PublishPayload(byte[] payload)
        {
            Payload = payload;
        }

        public override void Encode(IByteBuffer buffer, VariableHeader variableHeader)
        {
            if (Payload != null)
                buffer.WriteBytes(Payload);
        }

        public override void Decode(IByteBuffer buffer, VariableHeader variableHeader, ref int remainingLength)
        {
            Payload = buffer.ReadSliceArray(ref remainingLength);
        }
    }
}
