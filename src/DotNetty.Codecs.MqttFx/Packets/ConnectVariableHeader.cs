using DotNetty.Buffers;

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

        public void Encode(IByteBuffer buffer)
        {
            buffer.WriteString(ProtocolName);         // byte 1 - 8
            buffer.WriteByte(ProtocolLevel);          // byte 9

            // connect flags                          // byte 10
            var flags = UsernameFlag.ToByte() << 7;
            flags |= PasswordFlag.ToByte() << 6;
            flags |= WillRetain.ToByte() << 5;
            flags |= ((byte)WillQos) << 3;
            flags |= WillFlag.ToByte() << 2;
            flags |= CleanSession.ToByte() << 1;
            buffer.WriteByte((byte)flags);

            // keep alive
            buffer.WriteShort(KeepAlive);            // byte 11 - 12
        }
    }
}
