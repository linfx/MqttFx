using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 可变报头(CONNACK Packet variable header)
    /// </summary>
    public class ConnAckVariableHeader : VariableHeader
    {
        /*
         * 连接确认标志(3.2.2.1 Connect Acknowledge Flags)
         * Byte 1 is the "Connect Acknowledge Flags". Bits 7-1 are reserved and MUST be set to 0. 
         * Bit 0 (SP1) is the Session Present Flag.
        */

        /// <summary>
        /// 当前会话 Session Present
        /// </summary>
        public bool SessionPresent { get; set; }

        /// <summary>
        /// 连接返回码(Connect Return code)
        /// </summary>
        public ConnectReturnCode ConnectReturnCode { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="buffer"></param>
        public override void Encode(IByteBuffer buffer)
        {
            buffer.WriteByte(SessionPresent ? 0x01 : 0x00);
            buffer.WriteByte((byte)ConnectReturnCode);
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="buffer"></param>
        public override void Decode(IByteBuffer buffer, ref FixedHeader fixedHeader)
        {
            SessionPresent = (buffer.ReadByte(ref fixedHeader.RemainingLength) & 0x01) == 0x01;
            ConnectReturnCode = (ConnectReturnCode)buffer.ReadByte(ref fixedHeader.RemainingLength);
        }
    }
}
