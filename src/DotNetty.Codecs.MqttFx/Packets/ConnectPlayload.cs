using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    public struct ConnectPlayload
    {
        /// <summary>
        /// 客户端标识符 Client Identifier
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// 遗嘱主题 Will Topic
        /// </summary>
        public string WillTopic { get; set; }
        /// <summary>
        /// 遗嘱消息 Will Message
        /// </summary>
        public byte[] WillMessage { get; set; }
        /// <summary>
        /// 用户名 User Name
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 密码 Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="variableHeader"></param>
        public void Encode(IByteBuffer buffer, ConnectVariableHeader variableHeader)
        {
            buffer.WriteString(ClientId);
            if (variableHeader.WillFlag)
            {
                buffer.WriteString(WillTopic);
                buffer.WriteBytes(WillMessage);
            }
            if (variableHeader.UsernameFlag)
            {
                buffer.WriteString(UserName);
            }
            if (variableHeader.PasswordFlag)
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
        public void Decode(IByteBuffer buffer, FixedHeader fixedHeader, ConnectVariableHeader variableHeader)
        {
            int remainingLength = fixedHeader.RemaingLength;
            ClientId = buffer.ReadString(ref remainingLength);
            if (variableHeader.WillFlag)
            {
                WillTopic = buffer.ReadString(ref remainingLength);
                //WillMessage = buffer.ReadBytes(3);
            }
        }
    }
}
