using System;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 连接返回码
    /// </summary>
    [Flags]
    public enum ConnectReturnCode : byte
    {
        /// <summary>
        /// 连接已接受
        /// </summary>
        CONNECTION_ACCEPTED = 0x00,

        /// <summary>
        /// 连接已拒绝，不支持的协议版本
        /// </summary>
        CONNECTION_REFUSED_UNACCEPTABLE_PROTOCOL_VERSION = 0x01,

        /// <summary>
        /// 接已拒绝，不合格的客户端标识符
        /// </summary>
        CONNECTION_REFUSED_IDENTIFIER_REJECTED = 0x02,

        /// <summary>
        /// 连接已拒绝，服务端不可用
        /// </summary>
        CONNECTION_REFUSED_SERVER_UNAVAILABLE = 0x03,

        /// <summary>
        /// 连接已拒绝，无效的用户名或密码
        /// </summary>
        CONNECTION_REFUSED_BAD_USER_NAME_OR_PASSWORD = 0x04,

        /// <summary>
        /// 连接已拒绝，未授权
        /// </summary>
        CONNECTION_REFUSED_NOT_AUTHORIZED = 0x05,
    }
}
