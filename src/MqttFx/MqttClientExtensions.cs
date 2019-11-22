using DotNetty.Codecs.MqttFx.Packets;
using System.Threading;
using System.Threading.Tasks;

namespace MqttFx
{
    public static class MqttClientExtensions
    {
        /// <summary>
        /// 发送并等待返回
        /// </summary>
        /// <typeparam name="TPacket"></typeparam>
        /// <param name="client"></param>
        /// <param name="packet"></param>
        /// <returns></returns>
        internal static Task<TPacket> SendAndReceiveAsync<TPacket>(this MqttClient client, Packet packet) where TPacket : Packet
        {
            return client.SendAndReceiveAsync<TPacket>(packet, CancellationToken.None);
        }
    }
}
