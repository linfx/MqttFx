using DotNetty.Codecs.MqttFx;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace MqttFx.Channels
{
    class MqttChannelInitializer : ChannelInitializer<ISocketChannel>
    {
        private readonly MqttClient client;
        private readonly MqttConnectResult connectFuture;

        public MqttChannelInitializer(MqttClient client, MqttConnectResult connectFuture)
        {
            this.client = client;
            this.connectFuture = connectFuture;
        }

        protected override void InitChannel(ISocketChannel ch)
        {
            ch.Pipeline.AddLast("mqttDecoder", new MqttDecoder(true, 256 * 1024));
            ch.Pipeline.AddLast("mqttEncoder", MqttEncoder.Instance);
            //ch.pipeline().addLast("idleStateHandler", new IdleStateHandler(MqttClientImpl.this.clientConfig.getTimeoutSeconds(), MqttClientImpl.this.clientConfig.getTimeoutSeconds(), 0));
            //ch.pipeline().addLast("mqttPingHandler", new MqttPingHandler(MqttClientImpl.this.clientConfig.getTimeoutSeconds()));
            ch.Pipeline.AddLast("mqttHandler", new MqttChannelHandler(client, connectFuture));
        }
    }
}
