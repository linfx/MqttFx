using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs.MqttFx;
using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;
using Moq;
using Xunit;

namespace MqttFx.Test
{
    public class MqttCodecTests
    {
        static readonly IByteBufferAllocator Allocator = new UnpooledByteBufferAllocator();

        readonly MqttDecoder serverDecoder;
        readonly MqttDecoder clientDecoder;
        readonly Mock<IChannelHandlerContext> contextMock;

        public MqttCodecTests()
        {
            serverDecoder = new MqttDecoder(true, 256 * 1024);
            clientDecoder = new MqttDecoder(false, 256 * 1024);
            contextMock = new Mock<IChannelHandlerContext>(MockBehavior.Strict);
            contextMock.Setup(x => x.Removed).Returns(false);
            contextMock.Setup(x => x.Allocator).Returns(UnpooledByteBufferAllocator.Default);
        }

        [Theory]
        [InlineData(false, ConnectReturnCode.ConnectionAccepted)]
        [InlineData(true, ConnectReturnCode.ConnectionAccepted)]
        [InlineData(false, ConnectReturnCode.UnacceptedProtocolVersion)]
        [InlineData(false, ConnectReturnCode.IdentifierRejected)]
        [InlineData(false, ConnectReturnCode.BrokerUnavailable)]
        [InlineData(false, ConnectReturnCode.BadUsernameOrPassword)]
        [InlineData(false, ConnectReturnCode.RefusedNotAuthorized)]
        public void TestConnAckMessage(bool sessionPresent, ConnectReturnCode returnCode)
        {
            var packet = new ConnAckPacket
            {
                SessionPresent = sessionPresent,
                ConnectReturnCode = returnCode
            };

            ConnAckPacket recoded = RecodePacket(packet, false, true);

            contextMock.Verify(x => x.FireChannelRead(It.IsAny<ConnAckPacket>()), Times.Once);
            Assert.Equal(packet.SessionPresent, recoded.SessionPresent);
            Assert.Equal(packet.ConnectReturnCode, recoded.ConnectReturnCode);
        }


        T RecodePacket<T>(T packet, bool useServer, bool explodeForDecode)
            where T : Packet
        {
            var output = new List<object>();
            MqttEncoder.DoEncode(Allocator, packet, output);

            T observedPacket = null;
            this.contextMock.Setup(x => x.FireChannelRead(It.IsAny<T>()))
                .Callback((object message) => observedPacket = Assert.IsAssignableFrom<T>(message))
                .Returns(this.contextMock.Object);

            foreach (IByteBuffer message in output)
            {
                MqttDecoder mqttDecoder = useServer ? this.serverDecoder : this.clientDecoder;
                if (explodeForDecode)
                {
                    while (message.IsReadable())
                    {
                        IByteBuffer finalBuffer = message.ReadBytes(1);
                        mqttDecoder.ChannelRead(this.contextMock.Object, finalBuffer);
                    }
                }
                else
                {
                    mqttDecoder.ChannelRead(this.contextMock.Object, message);
                }
            }
            return observedPacket;
        }
    }
}
