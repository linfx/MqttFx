namespace DotNetty.Codecs.MqttFx.Packets;

/// <summary>
/// PING请求(PING request)
/// The PINGREQ Packet is sent from a Client to the Server. It can be used to:
/// 1. Indicate to the Server that the Client is alive in the absence of any other Control Packets being sent from the Client to the Server.
/// 2. Request that the Server responds to confirm that it is alive.
/// 3. Exercise the network to indicate that the Network Connection is active.
/// </summary>
public sealed record PingReqPacket : Packet
{
    /*
     * PINGREQ 数据包从客户端发送到服务器。它可用于：
     *  1. 向服务器指示在没有从客户端向服务器发送任何其他控制数据包的情况下，客户端处于活动状态。
     *  2. 请求服务器响应以确认它处于活动状态。
     *  3. 执行网络以指示网络连接处于活动状态。
    */

    public static readonly PingReqPacket Instance = new PingReqPacket();
}
