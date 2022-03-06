using System;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 连接返回码(Connect Return code)
    /// If a server sends a CONNACK packet containing a non-zero return code it MUST then close the Network Connection [MQTT-3.2.2-5].
    /// </summary>
    [Flags]
    public enum ConnectReturnCode : byte
    {
        /// <summary>
        /// 连接已接受(0x00 Connection Accepted)
        /// Connection accepted
        /// </summary>
        CONNECTION_ACCEPTED = 0x00,

        /// <summary>
        /// 连接已拒绝，不支持的协议版本(0x01 Connection Refused, unacceptable protocol version)
        /// The Server does not support the level of the MQTT protocol requested by the Client
        /// </summary>
        CONNECTION_REFUSED_UNACCEPTABLE_PROTOCOL_VERSION = 0x01,

        /// <summary>
        /// 接已拒绝，不合格的客户端标识符(0x02 Connection Refused, identifier rejected)
        /// The Client identifier is correct UTF-8 but not allowed by the Server
        /// </summary>
        CONNECTION_REFUSED_IDENTIFIER_REJECTED = 0x02,

        /// <summary>
        /// 连接已拒绝，服务端不可用(0x03 Connection Refused, Server unavailable)
        /// The Network Connection has been made but the MQTT service is unavailable
        /// </summary>
        CONNECTION_REFUSED_SERVER_UNAVAILABLE = 0x03,

        /// <summary>
        /// 连接已拒绝，无效的用户名或密码(0x04 Connection Refused, bad user name or password)
        /// The data in the user name or password is malformed
        /// </summary>
        CONNECTION_REFUSED_BAD_USER_NAME_OR_PASSWORD = 0x04,

        /// <summary>
        /// 连接已拒绝，未授权(0x05 Connection Refused, not authorized)
        /// The Client is not authorized to connect
        /// </summary>
        CONNECTION_REFUSED_NOT_AUTHORIZED = 0x05,
    }
}
