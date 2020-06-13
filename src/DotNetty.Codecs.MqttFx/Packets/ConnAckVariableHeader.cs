using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    public struct ConnAckVariableHeader
    {
        /// <summary>
        /// 当前会话
        /// </summary>
        public bool SessionPresent { get; set; }
        /// <summary>
        /// 连接返回码
        /// </summary>
        public ConnectReturnCode ConnectReturnCode { get; set; }

        public void Encode(IByteBuffer buffer)
        {
            buffer.WriteByte(SessionPresent ? 0x01 : 0x00);
            buffer.WriteByte((byte)ConnectReturnCode);
        }

        public void Decode(IByteBuffer buffer)
        {
            SessionPresent = (buffer.ReadByte() & 0x01) == 0x01;
            ConnectReturnCode = (ConnectReturnCode)buffer.ReadByte();
        }
    }
}
