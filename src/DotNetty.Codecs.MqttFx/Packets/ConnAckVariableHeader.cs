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

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="buffer"></param>
        public void Encode(IByteBuffer buffer)
        {
            buffer.WriteByte(SessionPresent ? 0x01 : 0x00);
            buffer.WriteByte((byte)ConnectReturnCode);
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="buffer"></param>
        public void Decode(IByteBuffer buffer)
        {
            SessionPresent = (buffer.ReadByte() & 0x01) == 0x01;
            ConnectReturnCode = (ConnectReturnCode)buffer.ReadByte();
        }
    }
}
