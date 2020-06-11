using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using System;
using System.Net.Security;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 发起连接
    /// </summary>
    public sealed class ConnectPacket : Packet
    {
        public ConnectPacket()
            : base(PacketType.CONNECT)
        {
        }

        #region Variable header

        /// <summary>
        /// 协议名
        /// </summary>
        public string ProtocolName { get; set; } = "MQTT";
        /// <summary>
        /// 协议级别
        /// </summary>
        public byte ProtocolLevel { get; set; } = 0x04;
        /// <summary>
        /// 保持连接 
        /// </summary>
        public ushort KeepAlive { get; set; }

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
                // variable header
                buf.WriteString(ProtocolName);        //byte 1 - 8
                buf.WriteByte(ProtocolLevel);         //byte 9

                // connect flags                      //byte 10
                var flags = UsernameFlag.ToByte() << 7;
                flags |= PasswordFlag.ToByte() << 6;
                flags |= WillRetain.ToByte() << 5;
                flags |= ((byte)WillQos) << 3;
                flags |= WillFlag.ToByte() << 2;
                flags |= CleanSession.ToByte() << 1;
                buf.WriteByte((byte)flags);

                // keep alive
                buf.WriteShort(KeepAlive);            //byte 11 - 12

                // payload
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
                FixedHeader.WriteFixedHeader(buffer);
                buffer.WriteBytes(buf);
            }
            finally
            {
                buf?.SafeRelease();
            }
        }

        public override void Decode(IByteBuffer buffer)
        {
            int remainingLength = RemaingLength;

            // variable header
            ProtocolName = buffer.ReadString(ref remainingLength);
            ProtocolLevel = buffer.ReadByte();

            // connect flags                      //byte 10
            int connectFlags = buffer.ReadByte();
            CleanSession = (connectFlags & 0x02) == 0x02;
            WillFlag = (connectFlags & 0x04) == 0x04;
            if(WillFlag)
            {
                WillRetain = (connectFlags & 0x20) == 0x20;
                Qos = (MqttQos)((connectFlags & 0x18) >> 3);
                WillTopic = string.Empty;
            }

            // keep alive

            // payload
            ClientId = buffer.ReadString(ref remainingLength);
            if(WillFlag)
            {
                WillTopic = buffer.ReadString(ref remainingLength);
                //WillMessage = buffer.ReadBytes
            }
        }
    }

    /// <summary>
    /// 连接回执
    /// </summary>
    public sealed class ConnAckPacket : Packet
    {
        public ConnAckPacket()
            : base(PacketType.CONNACK)
        {
        }

        /// <summary>
        /// 当前会话
        /// </summary>
        public bool SessionPresent { get; set; }

        /// <summary>
        /// 连接返回码
        /// </summary>
        public ConnectReturnCode ConnectReturnCode { get; set; }

        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                if (SessionPresent)
                    buf.WriteByte(1);  // 7 reserved 0-bits and SP = 1
                else
                    buf.WriteByte(0);  // 7 reserved 0-bits and SP = 0

                buf.WriteByte((byte)ConnectReturnCode);

                FixedHeader.RemaingLength = buf.ReadableBytes;
                FixedHeader.WriteFixedHeader(buffer);
                buffer.WriteBytes(buf);
            }
            finally
            {
                buf?.SafeRelease();
            }
        }

        public override void Decode(IByteBuffer buffer)
        {
            int remainingLength = RemaingLength;
            int ackData = buffer.ReadUnsignedShort(ref remainingLength);
            SessionPresent = ((ackData >> 8) & 0x1) != 0;
            ConnectReturnCode = (ConnectReturnCode)(ackData & 0xFF);
            FixedHeader.RemaingLength = remainingLength;
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
        UnacceptableProtocolVersion = 0x01,

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
        NotAuthorized = 0x05,

        /// <summary>
        /// RefusedNotAuthorized
        /// </summary>
        RefusedNotAuthorized = 0x6
    }
}
