using System;

namespace MqttFx
{
    public class MqttClientOptions
    {
        /// <summary>
        /// 客户端标识
        /// </summary>
        public string ClientId { get; set; } = $"MqttFx_{Guid.NewGuid().GetHashCode() & ushort.MaxValue}";
        /// <summary>
        /// 清除标识
        /// </summary>
        public bool CleanSession { get; set; } = true;
        /// <summary>
        /// 保持连接 
        /// </summary>
        public short KeepAlive { get; set; } = 60;
        /// <summary>
        /// 案例凭证
        /// </summary>
        public MqttClientCredentials Credentials { get; set; }
        /// <summary>
        /// 遗嘱主题
        /// </summary>
        public Message WillMessage { get; set; }
        /// <summary>
        /// 响应超时
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(3);
        /// <summary>
        /// 服务器
        /// </summary>
        public string Server { get; set; } = "localhost";
        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; } = 1883;
    }
}
