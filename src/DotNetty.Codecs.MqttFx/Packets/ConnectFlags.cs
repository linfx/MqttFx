namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// Connect Flags
    /// </summary>
    public struct ConnectFlags
    {
        /// <summary>
        /// 用户名标志(User Name Flag)
        /// </summary>
        public bool UsernameFlag;

        /// <summary>
        /// 密码标志(Password Flag)
        /// </summary>
        public bool PasswordFlag;

        /// <summary>
        /// 遗嘱保留(Will Retain)
        /// </summary>
        public bool WillRetain;

        /// <summary>
        /// 遗嘱QoS(Will QoS)
        /// </summary>
        public MqttQos WillQos;

        /// <summary>
        /// 遗嘱标志(Will Flag)
        /// </summary>
        public bool WillFlag;

        /// <summary>
        /// 清理会话(Clean Session)
        /// 设置为 0，服务端必须基于当前会话（使用客户端标识符识别）的状态恢复与客户端的通信。
        /// 设置为 1，客户端和服务端必须丢弃之前的任何会话并开始一个新的会话。会话仅持续和网络连接同样长的时间。与这个会话关联的状态数据不能被任何之后的会话重用 [MQTT-3.1.2-6]。
        /// </summary>
        public bool CleanSession;
    }
}
