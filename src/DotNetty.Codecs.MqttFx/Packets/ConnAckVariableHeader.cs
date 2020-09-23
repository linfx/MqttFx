using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    public struct ConnAckVariableHeader
    {
        /*
         * 连接确认标志 Connect Acknowledge Flags
         * 第1个字节是 连接确认标志，位7-1是保留位且必须设置为0。 第0 (SP)位 是当前会话（Session Present）标志。
        */
        /// <summary>
        /// 当前会话 Session Present
        /// </summary>
        public bool SessionPresent { get; set; }

        /// <summary>
        /// 连接返回码 Connect Return code
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
