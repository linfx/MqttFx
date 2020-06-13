namespace DotNetty.Codecs.MqttFx.Packets
{
    public struct ConnectVariableHeader
    {
        /// <summary>
        /// 协议名
        /// </summary>
        public string ProtocolName { get; set; }
        /// <summary>
        /// 协议级别
        /// </summary>
        public byte ProtocolLevel { get; set; }
        /// <summary>
        /// 保持连接 
        /// </summary>
        public ushort KeepAlive { get; set; }
        /// <summary>
        /// 用户名标志
        /// </summary>
        public bool UsernameFlag { get; set; }
        /// <summary>
        /// 密码标志
        /// </summary>
        public bool PasswordFlag { get; set; }
        /// <summary>
        /// 遗嘱保留
        /// </summary>
        public bool WillRetain { get; set; }
        /// <summary>
        /// 遗嘱QoS
        /// </summary>
        public MqttQos WillQos { get; set; }
        /// <summary>
        /// 遗嘱标志
        /// </summary>
        public bool WillFlag { get; set; }
        /// <summary>
        /// 清理会话
        /// </summary>
        public bool CleanSession { get; set; }
    }
}
