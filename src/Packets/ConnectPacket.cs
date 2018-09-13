using DotNetty.Buffers;
using nMqtt.Protocol;

namespace nMqtt.Packets
{
    /// <summary>
    /// 发起连接
    /// </summary>
    [PacketType(PacketType.CONNECT)]
    internal sealed class ConnectPacket : Packet
    {
        #region Variable header

        /// <summary>
        /// 协议名
        /// </summary>
        public string ProtocolName { get; } = "MQTT";
        /// <summary>
        /// 协议级别
        /// </summary>
        public byte ProtocolLevel { get; } = 0x04;
        /// <summary>
        /// 保持连接 
        /// </summary>
        public short KeepAlive { get; set; }

        #region Connect Flags
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
        #endregion

        #endregion

        #region Payload

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

        #endregion

        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                //variable header
                buf.WriteString(ProtocolName);        //byte 1 - 8
                buf.WriteByte(ProtocolLevel);         //byte 9

                //connect flags;                      //byte 10
                var flags = UsernameFlag.ToByte() << 7;
                flags |= PasswordFlag.ToByte() << 6;
                flags |= WillRetain.ToByte() << 5;
                flags |= ((byte)WillQos) << 3;
                flags |= WillFlag.ToByte() << 2;
                flags |= CleanSession.ToByte() << 1;
                buf.WriteByte((byte)flags);

                //keep alive
                buf.WriteShort(KeepAlive);            //byte 11 - 12

                //payload
                buf.WriteString(ClientId);
                if (WillFlag)
                {
                    buf.WriteString(WillTopic);
                    buf.WriteBytes(WillMessage);
                }
                if (UsernameFlag && PasswordFlag)
                {
                    buf.WriteString(UserName);
                    buf.WriteString(Password);
                }

                FixedHeader.RemaingLength = buf.ReadableBytes;
                FixedHeader.WriteTo(buffer);
                buffer.WriteBytes(buf);
            }
            finally
            {
                buf?.Release();
                buf = null;
            }
        }
    }

    /// <summary>
    /// 连接回执
    /// </summary>
    [PacketType(PacketType.CONNACK)]
    internal sealed class ConnAckPacket : Packet
    {
        /// <summary>
        /// 当前会话
        /// </summary>
        public bool SessionPresent { get; set; }
        /// <summary>
        /// 连接返回码
        /// </summary>
        public ConnectReturnCode ConnectReturnCode { get; set; }

        public override void Decode(IByteBuffer buffer)
        {
            SessionPresent = (buffer.ReadByte() & 0x01) == 1;
            ConnectReturnCode = (ConnectReturnCode)buffer.ReadByte();
        }
    }
}
