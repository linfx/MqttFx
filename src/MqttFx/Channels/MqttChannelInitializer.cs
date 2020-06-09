using DotNetty.Codecs.MqttFx;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System.Threading.Tasks;

namespace MqttFx.Channels
{
    internal class MqttChannelInitializer : ChannelInitializer<ISocketChannel>
    {
        private readonly IMqttClient client;
        private readonly TaskCompletionSource<MqttConnectResult> connectFuture;

        public MqttChannelInitializer(IMqttClient client, TaskCompletionSource<MqttConnectResult> connectFuture)
        {
            this.client = client;
            this.connectFuture = connectFuture;
        }

        protected override void InitChannel(ISocketChannel ch)
        {
            ch.Pipeline.AddLast(new LoggingHandler());
            ch.Pipeline.AddLast(MqttEncoder.Instance, new MqttDecoder(false, 256 * 1024));
            ch.Pipeline.AddLast(new IdleStateHandler(10, 10, 0), new MqttPingHandler());
            ch.Pipeline.AddLast(new MqttChannelHandler(client, connectFuture));
        }
    }
}
