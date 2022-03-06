using System;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 服务质量等级
    /// </summary>
    [Flags]
    public enum MqttQos : byte
    {
        /// <summary>
        /// 最多分发一次
        /// QOS Level 0 - Message is not guaranteed delivery. No retries are made to ensure delivery is successful.
        /// </summary>
        AtMostOnce = 0x00,

        /// <summary>
        /// 至少分发一次
        /// QOS Level 1 - Message is guaranteed delivery. It will be delivered at least one time, but may be delivered
        /// more than once if network errors occur.
        /// </summary>
        AtLeastOnce = 0x01,

        /// <summary>
        /// 只分发一次
        /// QOS Level 2 - Message will be delivered once, and only once. Message will be retried until
        /// it is successfully sent..
        /// </summary>
        ExactlyOnce = 0x02,

        /// <summary>
        /// FAILURE
        /// </summary>
        Failure = 0x80,
    }
}
