using System;

namespace MqttFx.Protocol
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
        ConnectionAccepted = 0x00,
        /// <summary>
        /// 连接已拒绝，不支持的协议版本
        /// </summary>
        UnacceptedProtocolVersion = 0x01,
        /// <summary>
        /// 接已拒绝，不合格的客户端标识符
        /// </summary>
        IdentifierRejected = 0x02,
        /// <summary>
        /// 连接已拒绝，服务端不可用
        /// </summary>
        BrokerUnavailable = 0x03,
        /// <summary>
        /// 连接已拒绝，无效的用户名或密码
        /// </summary>
        BadUsernameOrPassword = 0x04,
        /// <summary>
        /// 连接已拒绝，未授权
        /// </summary>
        NotAuthorized = 0x05,
        RefusedNotAuthorized = 0x6
    }
}
