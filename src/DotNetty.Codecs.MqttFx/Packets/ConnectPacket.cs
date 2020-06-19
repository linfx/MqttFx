using DotNetty.Buffers;
using DotNetty.Common.Utilities;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 连接报文
    /// </summary>
    public sealed class ConnectPacket : Packet
    {
        /// <summary>
        /// 可变报头
        /// </summary>
        public ConnectVariableHeader VariableHeader;

        /// <summary>
        /// 有效载荷
        /// </summary>
        public ConnectPlayload Payload;

        /// <summary>
        /// 连接报文
        /// </summary>
        public ConnectPacket()
            : base(PacketType.CONNECT)
        {
            VariableHeader.ProtocolName = "MQTT";
            VariableHeader.ProtocolLevel = 0x04;
        }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="buffer"></param>
        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                VariableHeader.Encode(buf);
                Payload.Encode(buf, VariableHeader);
                FixedHeader.Encode(buffer, buf.ReadableBytes);
                buffer.WriteBytes(buf);
            }
            finally
            {
                buf?.SafeRelease();
            }
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="buffer"></param>
        public override void Decode(IByteBuffer buffer)
        {
            VariableHeader.Decode(buffer, FixedHeader);
            Payload.Decode(buffer, FixedHeader, VariableHeader);
        }
    }

    //public sealed class ConnectPacket : Packet
    //{
    //    public ConnectPacket()
    //        : base(PacketType.CONNECT)
    //    {
    //    }

    //    #region Variable header

    //    /// <summary>
    //    /// 协议名
    //    /// </summary>
    //    public string ProtocolName { get; set; } = "MQTT";
    //    /// <summary>
    //    /// 协议级别
    //    /// </summary>
    //    public byte ProtocolLevel { get; set; } = 0x04;
    //    /// <summary>
    //    /// 保持连接 
    //    /// </summary>
    //    public ushort KeepAlive { get; set; }

    //    #region Connect Flags
    //    /// <summary>
    //    /// 用户名标志
    //    /// </summary>
    //    public bool UsernameFlag { get; set; }
    //    /// <summary>
    //    /// 密码标志
    //    /// </summary>
    //    public bool PasswordFlag { get; set; }
    //    /// <summary>
    //    /// 遗嘱保留
    //    /// </summary>
    //    public bool WillRetain { get; set; }
    //    /// <summary>
    //    /// 遗嘱QoS
    //    /// </summary>
    //    public MqttQos WillQos { get; set; }
    //    /// <summary>
    //    /// 遗嘱标志
    //    /// </summary>
    //    public bool WillFlag { get; set; }
    //    /// <summary>
    //    /// 清理会话
    //    /// </summary>
    //    public bool CleanSession { get; set; }
    //    #endregion

    //    #endregion

    //    #region Payload

    //    /// <summary>
    //    /// 客户端标识符 Client Identifier
    //    /// </summary>
    //    public string ClientId { get; set; }
    //    /// <summary>
    //    /// 遗嘱主题 Will Topic
    //    /// </summary>
    //    public string WillTopic { get; set; }
    //    /// <summary>
    //    /// 遗嘱消息 Will Message
    //    /// </summary>
    //    public byte[] WillMessage { get; set; }
    //    /// <summary>
    //    /// 用户名 User Name
    //    /// </summary>
    //    public string UserName { get; set; }
    //    /// <summary>
    //    /// 密码 Password
    //    /// </summary>
    //    public string Password { get; set; }

    //    #endregion

    //    public override void Encode(IByteBuffer buffer)
    //    {
    //        var buf = Unpooled.Buffer();
    //        try
    //        {
    //            // variable header
    //            buf.WriteString(ProtocolName);        //byte 1 - 8
    //            buf.WriteByte(ProtocolLevel);         //byte 9

    //            // connect flags;                      //byte 10
    //            var flags = UsernameFlag.ToByte() << 7;
    //            flags |= PasswordFlag.ToByte() << 6;
    //            flags |= WillRetain.ToByte() << 5;
    //            flags |= ((byte)WillQos) << 3;
    //            flags |= WillFlag.ToByte() << 2;
    //            flags |= CleanSession.ToByte() << 1;
    //            buf.WriteByte((byte)flags);

    //            // keep alive
    //            buf.WriteShort(KeepAlive);            //byte 11 - 12

    //            // payload
    //            buf.WriteString(ClientId);
    //            if (WillFlag)
    //            {
    //                buf.WriteString(WillTopic);
    //                buf.WriteBytes(WillMessage);
    //            }
    //            if (UsernameFlag && PasswordFlag)
    //            {
    //                buf.WriteString(UserName);
    //                buf.WriteString(Password);
    //            }

    //            FixedHeader.RemaingLength = buf.ReadableBytes;
    //            FixedHeader.WriteFixedHeader(buffer);
    //            buffer.WriteBytes(buf);
    //        }
    //        finally
    //        {
    //            buf?.SafeRelease();
    //        }
    //    }
    //}
}
