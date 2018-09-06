using System;

namespace nMqtt.Protocol
{
    /// <summary>
    /// 报文类型
    /// </summary>
    [Flags]
    public enum PacketType : byte
    {
        /// <summary>
        /// 发起连接
        /// </summary>
        CONNECT = 1,
        /// <summary>
        /// 连接回执
        /// </summary>
        CONNACK = 2,
        /// <summary>
        /// 发布消息
        /// </summary>
        PUBLISH = 3,
        /// <summary>
        /// 发布回执
        /// </summary>
        PUBACK = 4,
        /// <summary>
        /// QoS2消息回执
        /// </summary>
        PUBREC = 5,
        /// <summary>
        /// QoS2消息释放
        /// </summary>
        PUBREL = 6,
        /// <summary>
        /// QoS2消息完成
        /// </summary>
        PUBCOMP = 7,
        /// <summary>
        /// 订阅主题
        /// </summary>
        SUBSCRIBE = 8,
        /// <summary>
        /// 订阅回执
        /// </summary>
        SUBACK = 9,
        /// <summary>
        /// 取消订阅
        /// </summary>
        UNSUBSCRIBE = 10,
        /// <summary>
        /// 取消订阅回执
        /// </summary>
        UNSUBACK = 11,
        /// <summary>
        /// PING请求
        /// </summary>
        PINGREQ = 12,
        /// <summary>
        /// PING响应
        /// </summary>
        PINGRESP = 13,
        /// <summary>
        /// 断开连接
        /// </summary>
        DISCONNECT = 14
    }


    /// <summary>
    /// 报文类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PacketTypeAttribute : Attribute
    {
        public PacketTypeAttribute(PacketType packetType)
        {
            PacketType = packetType;
        }

        /// <summary>
        /// 报文类型
        /// </summary>
        public PacketType PacketType { get; set; }
    }
}
