namespace DotNetty.Codecs.MqttFx.Packets
{
    public struct ConnAckVariableHeader
    {
        /// <summary>
        /// 当前会话
        /// </summary>
        public bool SessionPresent { get; set; }

        /// <summary>
        /// 连接返回码
        /// </summary>
        public ConnectReturnCode ConnectReturnCode { get; set; }
    }
}
