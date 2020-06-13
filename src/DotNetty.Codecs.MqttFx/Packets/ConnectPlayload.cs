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

        public void Encode(IByteBuffer buffer, ConnectVariableHeader variableHeader)
        {
            buffer.WriteString(ClientId);
            if (variableHeader.WillFlag)
            {
                buffer.WriteString(WillTopic);
                buffer.WriteBytes(WillMessage);
            }
            if (variableHeader.UsernameFlag && variableHeader.PasswordFlag)
            {
                buffer.WriteString(UserName);
                buffer.WriteString(Password);
            }
        }

        public void Decode(IByteBuffer buffer)
        {
        }
    }
}
