using System;

namespace DotNetty.Codecs.MqttFx.Packets;

/// <summary>
/// 报文类型(MQTT Control Packet type)
/// </summary>
[Flags]
public enum PacketType : byte
{
    /// <summary>
    /// 发起连接(Client request to connect to Server)
    /// </summary>
    CONNECT = 1,

    /// <summary>
    /// 连接回执(Connect acknowledgment)
    /// </summary>
    CONNACK = 2,

    /// <summary>
    /// 发布消息(Publish message)
    /// </summary>
    PUBLISH = 3,

    /// <summary>
    /// 发布回执(Publish acknowledgment)
    /// </summary>
    PUBACK = 4,

    /// <summary>
    /// QoS2消息回执(Publish received)
    /// </summary>
    PUBREC = 5,

    /// <summary>
    /// QoS2消息释放(Publish release)
    /// </summary>
    PUBREL = 6,

    /// <summary>
    /// QoS2消息完成(Publish complet)
    /// </summary>
    PUBCOMP = 7,

    /// <summary>
    /// 订阅主题(Client subscribe request)
    /// </summary>
    SUBSCRIBE = 8,

    /// <summary>
    /// 订阅回执(Subscribe acknowledgment)
    /// </summary>
    SUBACK = 9,

    /// <summary>
    /// 取消订阅(Unsubscribe request)
    /// </summary>
    UNSUBSCRIBE = 10,

    /// <summary>
    /// 取消订阅回执(Unsubscribe acknowledgment)
    /// </summary>
    UNSUBACK = 11,

    /// <summary>
    /// PING请求(PING request)
    /// </summary>
    PINGREQ = 12,

    /// <summary>
    /// PING响应(PING response)
    /// </summary>
    PINGRESP = 13,

    /// <summary>
    /// 断开连接(Client is disconnecting)
    /// </summary>
    DISCONNECT = 14
}
