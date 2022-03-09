using DotNetty.Codecs.MqttFx.Packets;
using MqttFx.Client;
using System;

namespace MqttFx
{
    /// <summary>
    /// Options for <see cref="MqttClient"/>
    /// </summary>
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
        ///     Gets or sets the keep alive period.
        ///     The connection is normally left open by the client so that is can send and receive data at any time.
        ///     If no data flows over an open connection for a certain time period then the client will generate a PINGREQ and
        ///     expect to receive a PINGRESP from the broker.
        ///     This message exchange confirms that the connection is open and working.
        ///     This period is known as the keep alive period.
        /// </summary>
        public ushort KeepAlive { get; set; } = 15;

        /// <summary>
        /// 凭证
        /// </summary>
        public MqttClientCredentials Credentials { get; set; }

        /// <summary>
        ///     Gets or sets the retain flag of the will message.
        /// </summary>
        public bool WillRetain { get; set; }

        /// <summary>
        ///     Gets or sets the QoS level of the will message.
        /// </summary>
        public MqttQos WillQos { get; set; }

        /// <summary>
        ///     Gets or sets the topic of the will message.
        /// </summary>
        public string WillTopic { get; set; }

        /// <summary>
        ///     Gets or sets the payload of the will message.
        /// </summary>
        public byte[] WillPayload { get; set; }

        /// <summary>
        ///     Gets or sets the timeout which will be applied at socket level and internal operations.
        ///     The default value is the same as for sockets in .NET in general.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

        /// <summary>
        /// HostNameOrAddress
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; } = 1883;
    }
}
