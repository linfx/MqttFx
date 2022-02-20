using DotNetty.Buffers;
using DotNetty.Codecs.MqttFx;
using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MqttFx.Test
{
    public class MqttCodecTests
    {
        private static readonly IByteBufferAllocator Allocator = new UnpooledByteBufferAllocator();

        private readonly MqttDecoder serverDecoder;
        private readonly MqttDecoder clientDecoder;
        private readonly Mock<IChannelHandlerContext> contextMock;

        public MqttCodecTests()
        {
            serverDecoder = new MqttDecoder(true, 256 * 1024);
            clientDecoder = new MqttDecoder(false, 256 * 1024);
            contextMock = new Mock<IChannelHandlerContext>(MockBehavior.Strict);
            contextMock.Setup(x => x.Removed).Returns(false);
            contextMock.Setup(x => x.Allocator).Returns(UnpooledByteBufferAllocator.Default);
        }

        [Theory]
        [InlineData("a", true, 0, null, null, "will/topic/name", new byte[] { 5, 3, 255, 6, 5 }, MqttQos.EXACTLY_ONCE, true)]
        [InlineData("11a_2", false, 1, "user1", null, "will", new byte[0], MqttQos.AT_LEAST_ONCE, false)]
        [InlineData("abc/ж", false, 10, "", "pwd", null, null, null, false)]
        [InlineData("", true, 1000, "имя", "密碼", null, null, null, false)]
        public void ConnectMessageTest(string clientId, bool cleanSession, ushort keepAlive, string userName, string password, string willTopicName, byte[] willMessage, MqttQos? willQos, bool willRetain)
        {
            var packet = new ConnectPacket();
            var packet_variableHeader = (ConnectVariableHeader)packet.VariableHeader;
            var packet_payload = (ConnectPlayload)packet.Payload;

            packet_payload.ClientId = clientId;
            packet_variableHeader.ConnectFlags.CleanSession = cleanSession;
            packet_variableHeader.KeepAlive = keepAlive;
            if (userName != null)
            {
                packet_variableHeader.ConnectFlags.UsernameFlag = true;
                packet_payload.UserName = userName;
            }
            if (password != null)
            {
                packet_variableHeader.ConnectFlags.PasswordFlag = true;
                packet_payload.Password = password;
            }
            if (willTopicName != null)
            {
                packet_variableHeader.ConnectFlags.WillFlag = true;
                packet_variableHeader.ConnectFlags.WillQos = willQos ?? MqttQos.AT_MOST_ONCE;
                packet_variableHeader.ConnectFlags.WillRetain = willRetain;
                packet_payload.WillTopic = willTopicName;
                packet_payload.WillMessage = willMessage;
            }

            var recoded = RecodePacket(packet, true, false);
            var recoded_variableHeader = (ConnectVariableHeader)recoded.VariableHeader;
            var recoded_payload = (ConnectPlayload)recoded.Payload;

            contextMock.Verify(x => x.FireChannelRead(It.IsAny<ConnectPacket>()), Times.Once);
            Assert.Equal(packet_payload.ClientId, recoded_payload.ClientId);
            //Assert.Equal(packet.VariableHeader.CleanSession, recoded.VariableHeader.CleanSession);
            //Assert.Equal(packet.VariableHeader.KeepAlive, recoded.VariableHeader.KeepAlive);
            //Assert.Equal(packet.VariableHeader.UsernameFlag, recoded.VariableHeader.UsernameFlag);
            //if (packet.VariableHeader.UsernameFlag)
            //{
            //    Assert.Equal(packet.Payload.UserName, recoded.Payload.UserName);
            //}
            //Assert.Equal(packet.VariableHeader.PasswordFlag, recoded.VariableHeader.PasswordFlag);
            //if (packet.VariableHeader.PasswordFlag)
            //{
            //    Assert.Equal(packet.Payload.Password, recoded.Payload.Password);
            //}
            //if (packet.VariableHeader.WillFlag)
            //{
            //    Assert.Equal(packet.VariableHeader.WillQos, recoded.VariableHeader.WillQos);
            //    Assert.Equal(packet.VariableHeader.WillRetain, recoded.VariableHeader.WillRetain);
            //    Assert.Equal(packet.Payload.WillTopic, recoded.Payload.WillTopic);
            //    //Assert.True(ByteBufferUtil.Equals(Unpooled.WrappedBuffer(willMessage), recoded.WillMessage));
            //}
        }

        [Theory]
        [InlineData(false, ConnectReturnCode.CONNECTION_ACCEPTED)]
        [InlineData(true, ConnectReturnCode.CONNECTION_ACCEPTED)]
        [InlineData(false, ConnectReturnCode.CONNECTION_REFUSED_UNACCEPTABLE_PROTOCOL_VERSION)]
        [InlineData(false, ConnectReturnCode.CONNECTION_REFUSED_IDENTIFIER_REJECTED)]
        [InlineData(false, ConnectReturnCode.CONNECTION_REFUSED_SERVER_UNAVAILABLE)]
        [InlineData(false, ConnectReturnCode.CONNECTION_REFUSED_BAD_USER_NAME_OR_PASSWORD)]
        public void ConnAckMessageTest(bool sessionPresent, ConnectReturnCode returnCode)
        {
            var packet = new ConnAckPacket();
            var packet_variableHeader = (ConnAckVariableHeader)packet.VariableHeader;
            packet_variableHeader.SessionPresent = sessionPresent;
            packet_variableHeader.ConnectReturnCode = returnCode;

            var recoded = RecodePacket(packet, false, false);
            var recoded_variableHeader = (ConnAckVariableHeader)packet.VariableHeader;

            contextMock.Verify(x => x.FireChannelRead(It.IsAny<ConnAckPacket>()), Times.Once);
            Assert.Equal(packet_variableHeader.SessionPresent, recoded_variableHeader.SessionPresent);
            Assert.Equal(packet_variableHeader.ConnectReturnCode, recoded_variableHeader.ConnectReturnCode);
        }

        //[Theory]
        //[InlineData(1, new[] { "+", "+/+", "//", "/#", "+//+" }, new[] { MqttQos.EXACTLY_ONCE, MqttQos.AT_LEAST_ONCE, MqttQos.AT_MOST_ONCE, MqttQos.EXACTLY_ONCE, MqttQos.AT_MOST_ONCE })]
        //[InlineData(ushort.MaxValue, new[] { "a" }, new[] { MqttQos.AT_LEAST_ONCE })]
        //public void TestSubscribeMessage(ushort packetId, string[] topicFilters, MqttQos[] requestedQosValues)
        //{
        //    var packet = new SubscribePacket();
        //    packet.VariableHeader.PacketIdentifier = packetId;
        //    packet.AddRange(topicFilters.Zip(requestedQosValues, (topic, qos) => new SubscribeRequest(topic, qos)).ToArray());

        //    SubscribePacket recoded = RecodePacket(packet, true, true);

        //    contextMock.Verify(x => x.FireChannelRead(It.IsAny<SubscribePacket>()), Times.Once);
        //    //Assert.Equal(packet.Requests, recoded.Requests, EqualityComparer<SubscribeRequest>.Default);
        //    //Assert.Equal(packet.PacketId, recoded.PacketId);
        //}

        //[Theory]
        //[InlineData(1, new[] { MqttQos.ExactlyOnce, MqttQos.AtLeastOnce, MqttQos.AtMostOnce, MqttQos.Failure })]
        //[InlineData(ushort.MaxValue, new[] { MqttQos.AtLeastOnce })]
        //public void TestSubAckMessage(ushort packetId, MqttQos[] qosValues)
        //{
        //    var packet = new SubAckPacket
        //    {
        //        PacketId = packetId,
        //        ReturnCodes = qosValues,
        //    };

        //    SubAckPacket recoded = RecodePacket(packet, false, true);

        //    contextMock.Verify(x => x.FireChannelRead(It.IsAny<SubAckPacket>()), Times.Once);
        //    Assert.Equal(packet.ReturnCodes, recoded.ReturnCodes);
        //    Assert.Equal(packet.PacketId, recoded.PacketId);
        //}

        //[Theory]
        //[InlineData(1, new[] { "+", "+/+", "//", "/#", "+//+" })]
        //[InlineData(ushort.MaxValue, new[] { "a" })]
        //public void TestUnsubscribeMessage(ushort packetId, string[] topicFilters)
        //{
        //    var packet = new UnsubscribePacket
        //    {
        //        PacketId = packetId,
        //    };
        //    packet.AddRange(topicFilters);

        //    UnsubscribePacket recoded = RecodePacket(packet, true, true);

        //    contextMock.Verify(x => x.FireChannelRead(It.IsAny<UnsubscribePacket>()), Times.Once);
        //    //Assert.Equal(packet.TopicFilters, recoded.TopicFilters);
        //    Assert.Equal(packet.PacketId, recoded.PacketId);
        //}

        //[Theory]
        //[InlineData(MqttQos.AtMostOnce, false, false, 1, "a", null)]
        //[InlineData(MqttQos.ExactlyOnce, true, false, ushort.MaxValue, "/", new byte[0])]
        //[InlineData(MqttQos.AtLeastOnce, false, true, 129, "a/b", new byte[] { 1, 2, 3 })]
        //[InlineData(MqttQos.ExactlyOnce, true, true, ushort.MaxValue - 1, "topic/name/that/is/longer/than/256/characters/topic/name/that/is/longer/than/256/characters/topic/name/that/is/longer/than/256/characters/topic/name/that/is/longer/than/256/characters/topic/name/that/is/longer/than/256/characters/topic/name/that/is/longer/than/256/characters/", new byte[] { 1 })]
        //public void TestPublishMessage(MqttQos qos, bool dup, bool retain, ushort packetId, string topicName, byte[] payload)
        //{
        //    var packet = new PublishPacket(qos, dup, retain)
        //    {
        //        TopicName = topicName
        //    };
        //    if (qos > MqttQos.AtMostOnce)
        //    {
        //        packet.PacketId = packetId;
        //    }
        //    packet.Payload = payload;

        //    PublishPacket recoded = RecodePacket(packet, false, true);

        //    contextMock.Verify(x => x.FireChannelRead(It.IsAny<PublishPacket>()), Times.Once);
        //    Assert.Equal(packet.TopicName, recoded.TopicName);
        //    if (packet.Qos > MqttQos.AtMostOnce)
        //    {
        //        Assert.Equal(packet.PacketId, recoded.PacketId);
        //    }
        //    Assert.Equal(payload, recoded.Payload);
        //}

        [Fact]
        public void EmptyPacketMessagesTest()
        {
            EmptyPacketMessageTest(PingReqPacket.Instance, true);
            EmptyPacketMessageTest(PingRespPacket.Instance, false);
            EmptyPacketMessageTest(DisconnectPacket.Instance, true);
        }

        private void EmptyPacketMessageTest<T>(T packet, bool useServer) where T : Packet
        {
            T recoded = RecodePacket(packet, useServer, false);
            contextMock.Verify(x => x.FireChannelRead(It.IsAny<T>()), Times.Once);
        }

        private T RecodePacket<T>(T packet, bool useServer, bool explodeForDecode) where T : Packet
        {
            var output = new List<object>();
            MqttEncoder.DoEncode(Allocator, packet, output);

            T observedPacket = null;
            contextMock.Setup(x => x.FireChannelRead(It.IsAny<T>()))
                .Callback((object message) => observedPacket = Assert.IsAssignableFrom<T>(message))
                .Returns(contextMock.Object);

            foreach (IByteBuffer message in output)
            {
                MqttDecoder mqttDecoder = useServer ? serverDecoder : clientDecoder;
                if (explodeForDecode)
                {
                    while (message.IsReadable())
                    {
                        IByteBuffer finalBuffer = message.ReadBytes(1);
                        mqttDecoder.ChannelRead(contextMock.Object, finalBuffer);
                    }
                }
                else
                {
                    mqttDecoder.ChannelRead(contextMock.Object, message);
                }
            }
            return observedPacket;
        }
    }
}
