using System.IO;
using DotNetty.Buffers;
using nMqtt.Protocol;

namespace nMqtt.Packets
{
    /// <summary>
    /// 发起连接
    /// </summary>
    [PacketType(PacketType.CONNECT)]
    public sealed class ConnectPacket : Packet
    {
        #region 可变报头 Variable header
        /// <summary>
        /// 协议名
        /// </summary>
        public string ProtocolName { get; } = "MQTT";
        /// <summary>
        /// 协议级别
        /// </summary>
        public byte ProtocolLevel { get; } = 0x04;

        #region 连接标志 Connect Flags
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
        /// <summary>
        /// 保持连接 
        /// </summary>
        public short KeepAlive { get; set; } 
        #endregion

        #region 有效载荷 Payload
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
        public string WillMessage { get; set; }
        /// <summary>
        /// 用户名 User Name
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 密码 Password
        /// </summary>
        public string Password { get; set; }  
        #endregion

        public override void Encode(Stream buffer)
        {
            using (var body = new MemoryStream())
            {
                //variable header
                body.WriteString(ProtocolName);       //byte 1 - 8
                body.WriteByte(ProtocolLevel);        //byte 9

                //connect flags;                      //byte 10
                var flags = UsernameFlag.ToByte() << 7;
                flags |= PasswordFlag.ToByte() << 6;
                flags |= WillRetain.ToByte() << 5;
                flags |= ((byte)WillQos) << 3;
                flags |= WillFlag.ToByte() << 2;
                flags |=  CleanSession.ToByte() << 1;
                body.WriteByte((byte)flags);

                //keep alive
                body.WriteShort(KeepAlive);      //byte 11 - 12

                //payload
                body.WriteString(ClientId);
                if (WillFlag)
                {
                    body.WriteString(WillTopic);
                    body.WriteString(WillMessage);
                }
                if (UsernameFlag)
                    body.WriteString(UserName);
                if (PasswordFlag)
                    body.WriteString(Password);

                FixedHeader.RemaingLength = (int)body.Length;
                FixedHeader.WriteTo(buffer);
                body.WriteTo(buffer);
            }
        }

        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                //variable header
                buf.WriteString(ProtocolName);       //byte 1 - 8
                buf.WriteByte(ProtocolLevel);        //byte 9

                //connect flags;                      //byte 10
                var flags = UsernameFlag.ToByte() << 7;
                flags |= PasswordFlag.ToByte() << 6;
                flags |= WillRetain.ToByte() << 5;
                flags |= ((byte)WillQos) << 3;
                flags |= WillFlag.ToByte() << 2;
                flags |= CleanSession.ToByte() << 1;
                buf.WriteByte((byte)flags);

                //keep alive
                buf.WriteShort(KeepAlive);      //byte 11 - 12

                //payload
                buf.WriteString(ClientId);
                if (WillFlag)
                {
                    buf.WriteString(WillTopic);
                    buf.WriteString(WillMessage);
                }
                if (UsernameFlag)
                    buf.WriteString(UserName);
                if (PasswordFlag)
                    buf.WriteString(Password);

                FixedHeader.RemaingLength = buf.WriterIndex;
                FixedHeader.WriteTo(buffer);
                buf.WriteBytes(buffer);
                buf = null;
            }
            finally
            {
                buf?.Release();
            }
        }
    }

    /// <summary>
    /// 连接回执
    /// </summary>
    [PacketType(PacketType.CONNACK)]
    public sealed class ConnAckPacket : Packet
    {
        /// <summary>
        /// 当前会话
        /// </summary>
        public bool SessionPresent { get; set; }
        /// <summary>
        /// 连接返回码
        /// </summary>
        public ConnectReturnCode ConnectReturnCode { get; set; }

        public override void Decode(Stream buffer)
        {
            SessionPresent = (buffer.ReadByte() & 0x01) == 1;
            ConnectReturnCode = (ConnectReturnCode)buffer.ReadByte();
        }

        public override void Decode(IByteBuffer buffer)
        {
            SessionPresent = (buffer.ReadByte() & 0x01) == 1;
            ConnectReturnCode = (ConnectReturnCode)buffer.ReadByte();
        }
    }
}
