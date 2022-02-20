using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 可变报头(Variable header)
    /// </summary>
    public class ConnectVariableHeader : VariableHeader
    {
        /// <summary>
        /// 协议名(UTF-8编码的字符串)(Protocol Name)
        /// </summary>
        public string ProtocolName { get; set; } = "MQTT";

        /// <summary>
        /// 协议级别
        /// 3.1.1版协议，协议级别字段的值是(0x04)(Protocol Level)
        /// </summary>
        public byte ProtocolLevel { get; set; } = 0x04;

        /// <summary>
        /// Connect Flags
        /// </summary>
        public ConnectFlags ConnectFlags;

        /// <summary>
        /// 保持连接 
        /// 以秒为单位的时间间隔，表示为一个16位的字，它是指在客户端传输完成一个控制报文的时刻到发送下一个报文的时刻，两者之间允许空闲的最大时间间隔。
        /// </summary>
        public ushort KeepAlive { get; set; }

        /// <summary>
        /// CONNECT报文的可变报头按下列次序包含四个字段：
        /// 协议名（Protocol Name）
        /// 协议级别（Protocol Level）
        /// 连接标志（Connect Flags）
        /// 保持连接（Keep Alive）
        /// </summary>
        /// <param name="buffer"></param>
        public override void Encode(IByteBuffer buffer)
        {
            // 协议名 Protocol Name
            buffer.WriteString(ProtocolName);

            // 协议级别 Protocol Level
            buffer.WriteByte(ProtocolLevel);

            // 连接标志 Connect Flags                          
            var connectFlags = ConnectFlags.UsernameFlag.ToByte() << 7;
            connectFlags |= ConnectFlags.PasswordFlag.ToByte() << 6;
            connectFlags |= ConnectFlags.WillRetain.ToByte() << 5;
            connectFlags |= ((byte)ConnectFlags.WillQos) << 3;
            connectFlags |= ConnectFlags.WillFlag.ToByte() << 2;
            connectFlags |= ConnectFlags.CleanSession.ToByte() << 1;
            buffer.WriteByte(connectFlags);

            // 保持连接 Keep Alive
            buffer.WriteShort(KeepAlive);
        }

        public override void Decode(IByteBuffer buffer, ref int remainingLength)
        {
            // 协议名 Protocol Name
            ProtocolName = buffer.ReadString(ref remainingLength);

            // 协议级别 Protocol Level
            ProtocolLevel = buffer.ReadByte(ref remainingLength);

            // 连接标志 Connect Flags
            int connectFlags = buffer.ReadByte(ref remainingLength);
            ConnectFlags.CleanSession = (connectFlags & 0x02) == 0x02;
            ConnectFlags.WillFlag = (connectFlags & 0x04) == 0x04;
            if (ConnectFlags.WillFlag)
            {
                ConnectFlags.WillQos = (MqttQos)((connectFlags & 0x18) >> 3);
                ConnectFlags.WillRetain = (connectFlags & 0x20) == 0x20;
            }
            ConnectFlags.UsernameFlag = (connectFlags & 0x80) == 0x80;
            ConnectFlags.PasswordFlag = (connectFlags & 0x40) == 0x40;

            // 保持连接 Keep Alive
            KeepAlive = (ushort)buffer.ReadShort(ref remainingLength);
        }
    }
}
