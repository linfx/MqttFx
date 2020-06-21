//using DotNetty.Buffers;

//namespace DotNetty.Codecs.MqttFx.Packets
//{
//    /// <summary>
//    /// 可变报头
//    /// </summary>
//    public struct PublishVariableHeader
//    {
//        /// <summary>
//        /// 主题名(UTF-8编码的字符串)
//        /// </summary>
//        public string TopicName { get; set; }

//        /// <summary>
//        /// 报文标识符
//        /// 只有当QoS等级是1或2时，报文标识符（Packet Identifier）字段才能出现在PUBLISH报文中。
//        /// </summary>
//        public ushort PacketIdentifier { get; set; }

//        /// <summary>
//        /// 编码
//        /// </summary>
//        /// <param name="buffer"></param>
//        /// <param name="fixedHeader"></param>
//        public void Encode(IByteBuffer buffer, FixedHeader fixedHeader)
//        {
//            buffer.WriteString(TopicName);
//            if (fixedHeader.Qos > MqttQos.AtLeastOnce)
//                buffer.WriteUnsignedShort(PacketIdentifier);
//        }

//        /// <summary>
//        /// 解码
//        /// </summary>
//        /// <param name="buffer"></param>
//        /// <param name="fixedHeader"></param>
//        public void Decode(IByteBuffer buffer, FixedHeader fixedHeader)
//        {
//            int remainingLength = fixedHeader.RemaingLength;
//            TopicName = buffer.ReadString(ref remainingLength);
//            if (fixedHeader.Qos > MqttQos.AtLeastOnce)
//            {
//                PacketIdentifier = buffer.ReadUnsignedShort(ref remainingLength);
//                if (PacketIdentifier == 0)
//                    throw new DecoderException("[MQTT-2.3.1-1]");
//            }
//            fixedHeader.RemaingLength = remainingLength;
//        }
//    }
//}
