using System;

namespace nMqtt
{
    public class MqttClientOptions
    {
        /// <summary>
        /// 客户端标识
        /// </summary>
        public string ClientId { get; set; } = Guid.NewGuid().ToString("N");
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

        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(60);

        public string Server { get; set; } = "localhost";

        public int Port { get; set; } = 1883;
    }
}
