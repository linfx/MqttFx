using System;

namespace MqttFx.Protocol
{
    /// <summary>
    /// 服务质量等级
    /// </summary>
    [Flags]
    public enum MqttQos : byte
    {
        /// <summary>
        ///     QOS Level 0 - Message is not guaranteed delivery. No retries are made to ensure delivery is successful.
        /// </summary>
        AtMostOnce = 0,
        /// <summary>
        ///     QOS Level 1 - Message is guaranteed delivery. It will be delivered at least one time, but may be delivered
        ///     more than once if network errors occur.
        /// </summary>
        AtLeastOnce = 1,
        /// <summary>
        ///     QOS Level 2 - Message will be delivered once, and only once. Message will be retried until
        ///     it is successfully sent..
        /// </summary>
        ExactlyOnce = 2,
    }
}
