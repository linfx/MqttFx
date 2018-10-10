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

        //[Theory]
        //[InlineData(1, new[] { "+", "+/+", "//", "/#", "+//+" }, new[] { MqttQos.ExactlyOnce, MqttQos.AtLeastOnce, MqttQos.AtMostOnce, MqttQos.ExactlyOnce, MqttQos.AtMostOnce })]
        //[InlineData(ushort.MaxValue, new[] { "a" }, new[] { MqttQos.AtLeastOnce })]
        //public void TestSubscribeMessage(int packetId, string[] topicFilters, MqttQos[] requestedQosValues)
        //{
        //    var packet = new SubscribePacket(packetId, topicFilters.Zip(requestedQosValues, (topic, qos) => new SubscriptionRequest(topic, qos)).ToArray());

        //    SubscribePacket recoded = this.RecodePacket(packet, true, true);

        //    this.contextMock.Verify(x => x.FireChannelRead(It.IsAny<SubscribePacket>()), Times.Once);
        //    Assert.Equal(packet.Requests, recoded.Requests, EqualityComparer<SubscriptionRequest>.Default);
        //    Assert.Equal(packet.PacketId, recoded.PacketId);
        //}

        [Theory]
        [InlineData(1, new[] { MqttQos.ExactlyOnce, MqttQos.AtLeastOnce, MqttQos.AtMostOnce, MqttQos.AtMostOnce })]
        [InlineData(ushort.MaxValue, new[] { MqttQos.AtLeastOnce })]
        public void TestSubAckMessage(ushort packetId, MqttQos[] qosValues)
        {
            var packet = new SubAckPacket
            {
                PacketId = packetId,
                //ReturnCodes = qosValues
            };

            SubAckPacket recoded = this.RecodePacket(packet, false, true);

            this.contextMock.Verify(x => x.FireChannelRead(It.IsAny<SubAckPacket>()), Times.Once);
            Assert.Equal(packet.ReturnCodes, recoded.ReturnCodes);
            Assert.Equal(packet.PacketId, recoded.PacketId);
        }

        [Theory]
        [InlineData(1, new[] { "+", "+/+", "//", "/#", "+//+" })]
        [InlineData(ushort.MaxValue, new[] { "a" })]
        public void TestUnsubscribeMessage(ushort packetId, string[] topicFilters)
        {
            var packet = new UnsubscribePacket
            {
                PacketId = packetId,
            };
            packet.AddRange(topicFilters);

            UnsubscribePacket recoded = this.RecodePacket(packet, true, true);

            this.contextMock.Verify(x => x.FireChannelRead(It.IsAny<UnsubscribePacket>()), Times.Once);
            //Assert.Equal(packet.TopicFilters, recoded.TopicFilters);
            Assert.Equal(packet.PacketId, recoded.PacketId);
        }


        [Theory]
        [InlineData(MqttQos.AtMostOnce, false, false, 1, "a", null)]
        [InlineData(MqttQos.ExactlyOnce, true, false, ushort.MaxValue, "/", new byte[0])]
        [InlineData(MqttQos.AtLeastOnce, false, true, 129, "a/b", new byte[] { 1, 2, 3 })]
        [InlineData(MqttQos.ExactlyOnce, true, true, ushort.MaxValue - 1, "topic/name/that/is/longer/than/256/characters/topic/name/that/is/longer/than/256/characters/topic/name/that/is/longer/than/256/characters/topic/name/that/is/longer/than/256/characters/topic/name/that/is/longer/than/256/characters/topic/name/that/is/longer/than/256/characters/", new byte[] { 1 })]
        public void TestPublishMessage(MqttQos qos, bool dup, bool retain, ushort packetId, string topicName, byte[] payload)
        {
            var packet = new PublishPacket(qos, dup, retain)
            {
                TopicName = topicName
            };
            if (qos > MqttQos.AtMostOnce)
            {
                packet.PacketId = packetId;
            }
            packet.Payload = payload;

            PublishPacket recoded = this.RecodePacket(packet, false, true);

            this.contextMock.Verify(x => x.FireChannelRead(It.IsAny<PublishPacket>()), Times.Once);
            Assert.Equal(packet.TopicName, recoded.TopicName);
            if (packet.Qos > MqttQos.AtMostOnce)
            {
                Assert.Equal(packet.PacketId, recoded.PacketId);
            }
            Assert.True(ByteBufferUtil.Equals(payload == null ? Unpooled.Empty : Unpooled.WrappedBuffer(payload), recoded.Payload));
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
