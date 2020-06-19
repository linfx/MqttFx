using System;

namespace DotNetty.Codecs.MqttFx.Packets
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
        CONNECT = 0x10,
        /// <summary>
        /// 连接回执
        /// </summary>
        CONNACK = 0x20,
        /// <summary>
        /// 发布消息
        /// </summary>
        PUBLISH = 0x30,
        /// <summary>
        /// 发布回执
        /// </summary>
        PUBACK = 0x40,
        /// <summary>
        /// QoS2消息回执
        /// </summary>
        PUBREC = 0x50,
        /// <summary>
        /// QoS2消息释放
        /// </summary>
        PUBREL = 0x60,
        /// <summary>
        /// QoS2消息完成
        /// </summary>
        PUBCOMP = 0x70,
        /// <summary>
        /// 订阅主题
        /// </summary>
        SUBSCRIBE = 0x80,
        /// <summary>
        /// 订阅回执
        /// </summary>
        SUBACK = 0x90,
        /// <summary>
        /// 取消订阅
        /// </summary>
        UNSUBSCRIBE = 0xa0,
        /// <summary>
        /// 取消订阅回执
        /// </summary>
        UNSUBACK = 0xb0,
        /// <summary>
        /// PING请求
        /// </summary>
        PINGREQ = 0xc0,
        /// <summary>
        /// PING响应
        /// </summary>
        PINGRESP = 0xd0,
        /// <summary>
        /// 断开连接
        /// </summary>
        DISCONNECT = 0xe0,
    }
}
