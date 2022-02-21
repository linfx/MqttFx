using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    public class PublishPayload : Payload
    {
        /// <summary>
        /// 有效载荷
        /// </summary>
        public byte[] Payload { get; set; }

        public override void Encode(IByteBuffer buffer, VariableHeader variableHeader)
        {
            if(Payload != null)
                buffer.WriteBytes(Payload);
        }

        public override void Decode(IByteBuffer buffer, VariableHeader variableHeader, ref int remainingLength)
        {
            IByteBuffer payload;
            if (remainingLength > 0)
            {
                payload = buffer.ReadSlice(remainingLength);
                payload.Retain();
                remainingLength = 0;
            }
            else
            {
                payload = Unpooled.Empty;
            }
            Payload = payload.Array;
        }
    }
}
