using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 有效载荷(Payload)
    /// </summary>
    public class ConnectPlayload : Payload
    {
        /// <summary>
        /// 客户端标识符(Client Identifier)
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// 遗嘱主题(Will Topic)
        /// </summary>
        public string WillTopic { get; set; }

        /// <summary>
        /// 遗嘱消息(Will Message)
        /// </summary>
        public byte[] WillMessage { get; set; }

        /// <summary>
        /// 用户名(User Name)
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码(Password)
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="variableHeader"></param>
        public override void Encode(IByteBuffer buffer, VariableHeader variableHeader)
        {
            var connectVariableHeader = (ConnectVariableHeader)variableHeader;

            buffer.WriteString(ClientId);
            if (connectVariableHeader.ConnectFlags.WillFlag)
            {
                buffer.WriteString(WillTopic);
                buffer.WriteBytes(WillMessage);
            }
            if (connectVariableHeader.ConnectFlags.UsernameFlag)
            {
                buffer.WriteString(UserName);
            }
            if (connectVariableHeader.ConnectFlags.PasswordFlag)
            {
                buffer.WriteString(Password);
            }
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="fixedHeader"></param>
        /// <param name="variableHeader"></param>
        public override void Decode(IByteBuffer buffer, VariableHeader variableHeader, ref int remainingLength)
        {
            var connectVariableHeader = (ConnectVariableHeader)variableHeader;

            ClientId = buffer.ReadString(ref remainingLength);
            if (connectVariableHeader.ConnectFlags.WillFlag)
            {
                WillTopic = buffer.ReadString(ref remainingLength);
                int willMessageLength = buffer.ReadUnsignedShort(ref remainingLength);
                WillMessage = buffer.ReadBytes(willMessageLength, ref remainingLength);
            }
        }
    }
}
