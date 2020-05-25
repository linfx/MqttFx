using DotNetty.Codecs.MqttFx;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Threading.Tasks;

namespace MqttFx.Channels
{
    internal class MqttChannelInitializer : ChannelInitializer<ISocketChannel>
    {
        private readonly IMqttClient client;
        private readonly TaskCompletionSource<MqttConnectResult> connectPromise;

        public MqttChannelInitializer(IMqttClient client, TaskCompletionSource<MqttConnectResult> connectPromise)
        {
            this.client = client;
            this.connectPromise = connectPromise;
        }

        protected override void InitChannel(ISocketChannel ch)
        {
            ch.Pipeline.AddLast(new LoggingHandler());
            ch.Pipeline.AddLast(MqttEncoder.Instance, new MqttDecoder(true, 256 * 1024), new MqttChannelHandler(client.Config, connectPromise));
            //ch.pipeline().addLast("idleStateHandler", new IdleStateHandler(MqttClientImpl.this.clientConfig.getTimeoutSeconds(), MqttClientImpl.this.clientConfig.getTimeoutSeconds(), 0));
            //ch.pipeline().addLast("mqttPingHandler", new MqttPingHandler(MqttClientImpl.this.clientConfig.getTimeoutSeconds()));
        }
    }
}
