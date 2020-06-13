using DotNetty.Buffers;
using DotNetty.Common.Utilities;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 发起连接
    /// </summary>
    public sealed class ConnectPacket : Packet
    {
        public ConnectPacket()
            : base(PacketType.CONNECT) { }

        /// <summary>
        /// 可变报头
        /// </summary>
        public ConnectVariableHeader VariableHeader;

        /// <summary>
        /// 载荷
        /// </summary>
        public ConnectPlayload Payload;

        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                // variable header
                buf.WriteString(VariableHeader.ProtocolName);        //byte 1 - 8
                buf.WriteByte(VariableHeader.ProtocolLevel);         //byte 9

                // connect flags                      //byte 10
                var flags = VariableHeader.UsernameFlag.ToByte() << 7;
                flags |= VariableHeader.PasswordFlag.ToByte() << 6;
                flags |= VariableHeader.WillRetain.ToByte() << 5;
                flags |= ((byte)VariableHeader.WillQos) << 3;
                flags |= VariableHeader.WillFlag.ToByte() << 2;
                flags |= VariableHeader.CleanSession.ToByte() << 1;
                buf.WriteByte((byte)flags);

                // keep alive
                buf.WriteShort(VariableHeader.KeepAlive);            //byte 11 - 12

                // payload
                buf.WriteString(Payload.ClientId);
                if (VariableHeader.WillFlag)
                {
                    buf.WriteString(Payload.WillTopic);
                    buf.WriteBytes(Payload.WillMessage);
                }
                if (VariableHeader.UsernameFlag && VariableHeader.PasswordFlag)
                {
                    buf.WriteString(Payload.UserName);
                    buf.WriteString(Payload.Password);
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
            VariableHeader.ProtocolName = buffer.ReadString(ref remainingLength);
            VariableHeader.ProtocolLevel = buffer.ReadByte();

            // connect flags                      //byte 10
            int connectFlags = buffer.ReadByte();
            VariableHeader.CleanSession = (connectFlags & 0x02) == 0x02;
            VariableHeader.WillFlag = (connectFlags & 0x04) == 0x04;
            if (VariableHeader.WillFlag)
            {
                VariableHeader.WillRetain = (connectFlags & 0x20) == 0x20;
                Qos = (MqttQos)((connectFlags & 0x18) >> 3);
                Payload.WillTopic = string.Empty;
            }

            // keep alive

            // payload
            Payload.ClientId = buffer.ReadString(ref remainingLength);
            if (VariableHeader.WillFlag)
            {
                Payload.WillTopic = buffer.ReadString(ref remainingLength);
                //WillMessage = buffer.ReadBytes
            }
        }
    }
}
