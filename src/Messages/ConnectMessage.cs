using System;
using System.IO;

namespace nMqtt.Messages
{
    /// <summary>
    /// 发起连接
    /// </summary>
    [MessageType(MqttMessageType.CONNECT)]
    public sealed class ConnectMessage : MqttMessage
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

        public override void Encode(Stream stream)
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
                FixedHeader.WriteTo(stream);
                body.WriteTo(stream);
            }
        }
    }

    /// <summary>
    /// 连接回执
    /// </summary>
    [MessageType(MqttMessageType.CONNACK)]
    public sealed class ConnAckMessage : MqttMessage
    {
        /// <summary>
        /// 当前会话
        /// </summary>
        public bool SessionPresent { get; set; }
        /// <summary>
        /// 连接返回码
        /// </summary>
        public ConnectReturnCode ConnectReturnCode { get; set; }

        public override void Decode(Stream stream)
        {
            SessionPresent = (stream.ReadByte() & 0x01) == 1;
            ConnectReturnCode = (ConnectReturnCode)stream.ReadByte();
        }
    }

    /// <summary>
    /// 连接返回码
    /// </summary>
    [Flags]
    public enum ConnectReturnCode : byte
    {
        /// <summary>
        /// 连接已接受
        /// </summary>
        ConnectionAccepted = 0x00,
        /// <summary>
        /// 连接已拒绝，不支持的协议版本
        /// </summary>
        UnacceptedProtocolVersion = 0x01,
        /// <summary>
        /// 接已拒绝，不合格的客户端标识符
        /// </summary>
        IdentifierRejected = 0x02,
        /// <summary>
        /// 连接已拒绝，服务端不可用
        /// </summary>
        BrokerUnavailable = 0x03,
        /// <summary>
        /// 连接已拒绝，无效的用户名或密码
        /// </summary>
        BadUsernameOrPassword = 0x04,
        /// <summary>
        /// 连接已拒绝，未授权
        /// </summary>
        NotAuthorized = 0x05
    }
}
