using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 可变报头
    /// </summary>
    public struct ConnectVariableHeader
    {
        /// <summary>
        /// 协议名(UTF-8编码的字符串)
        /// </summary>
        public string ProtocolName { get; set; }
        /// <summary>
        /// 协议级别
        /// 3.1.1版协议，协议级别字段的值是(0x04)
        /// </summary>
        public byte ProtocolLevel { get; set; }
        /// <summary>
        /// 保持连接 
        /// 以秒为单位的时间间隔，表示为一个16位的字，它是指在客户端传输完成一个控制报文的时刻到发送下一个报文的时刻，两者之间允许空闲的最大时间间隔。
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
        /// 设置为 0，服务端必须基于当前会话（使用客户端标识符识别）的状态恢复与客户端的通信。
        /// 设置为 1，客户端和服务端必须丢弃之前的任何会话并开始一个新的会话。会话仅持续和网络连接同样长的时间。与这个会话关联的状态数据不能被任何之后的会话重用 [MQTT-3.1.2-6]。
        /// </summary>
        public bool CleanSession { get; set; }

        /// <summary>
        /// CONNECT报文的可变报头按下列次序包含四个字段：
        /// 协议名（Protocol Name）
        /// 协议级别（Protocol Level）
        /// 连接标志（Connect Flags）
        /// 保持连接（Keep Alive）
        /// </summary>
        /// <param name="buffer"></param>
        public void Encode(IByteBuffer buffer)
        {
            // 协议名 Protocol Name
            // 协议级别 Protocol Level
            buffer.WriteString(ProtocolName);        
            buffer.WriteByte(ProtocolLevel);

            // 连接标志 Connect Flags                          
            var connectFlags = UsernameFlag.ToByte() << 7;
            connectFlags |= PasswordFlag.ToByte() << 6;
            connectFlags |= WillRetain.ToByte() << 5;
            connectFlags |= ((byte)WillQos) << 3;
            connectFlags |= WillFlag.ToByte() << 2;
            connectFlags |= CleanSession.ToByte() << 1;
            buffer.WriteByte(connectFlags);

            // 保持连接 Keep Alive
            buffer.WriteShort(KeepAlive);            
        }

        public void Decode(IByteBuffer buffer, FixedHeader fixedHeader)
        {
            int remainingLength = fixedHeader.RemaingLength;

            // 协议名 Protocol Name
            // 协议级别 Protocol Level
            ProtocolName = buffer.ReadString(ref remainingLength);
            ProtocolLevel = buffer.ReadByte();

            // 连接标志 Connect Flags
            int connectFlags = buffer.ReadByte();
            CleanSession = (connectFlags & 0x02) == 0x02;
            WillFlag = (connectFlags & 0x04) == 0x04;
            if (WillFlag)
            {
                WillRetain = (connectFlags & 0x20) == 0x20;
                WillQos = (MqttQos)((connectFlags & 0x18) >> 3);
                //WillTopic = string.Empty;
            }

            // 保持连接 Keep Alive
            KeepAlive = (ushort)buffer.ReadShort();
        }
    }
}
