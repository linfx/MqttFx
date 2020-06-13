namespace DotNetty.Codecs.MqttFx.Packets
{
    public struct PublishVariableHeader
    {
        /// <summary>
        /// 主题
        /// </summary>
        public string TopicName { get; set; }

        /// <summary>
        /// 报文标识符
        /// </summary>
        public ushort PacketIdentifier { get; set; }
    }
}
