using System;
using Xunit;
using NSubstitute;
using PDMHelpers;

namespace PDMMessages.UnitTests
{
    public class COPSPacketTest
    {
        string strIn = @"<m1 id=""163"" dst=""4""><m>0640JJG</m><d>111604180918</d><u>100206</u><o>1</o><ad>4</ad><g>60020</g><pt>3</pt><cdl>1</cdl><cad>0</cad><lr>0</lr><aft>0</aft><afm>0</afm></m1>";

        private COPSPacket CreatePacket()
        {
            ILoggerManager loggerManager = Substitute.For<ILoggerManager>();
            COPSPacket packet = new COPSPacket(loggerManager);
            return packet;
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Parse_EmptyInput_ThrowsArgumentNullException(string input)
        {
            COPSPacket packet = CreatePacket();

            Action codeToTest = () => packet.Parse(input);

            Assert.Throws<ArgumentNullException>(codeToTest);
        }
        
        [Fact]
        public void Parse_ValidInput_CreateMessages() {
            COPSPacket packet = CreatePacket();

            packet.Parse(strIn);
            Assert.Equal(1, packet.GetMsgNum());
            Assert.Equal(0, packet.GetMsg(0).GetMsgType());
            Assert.Equal("163", packet.GetMsg(0).fstrGetAtt("id"));
        }

        [Fact]
        public void Reset_WhenHasMessages_ShouldReturnZeroMsgCount()
        {
            COPSPacket packet = CreatePacket();

            packet.Parse(strIn);
            Assert.Equal(1, packet.GetMsgNum());
            packet.Reset();
            Assert.Equal(0, packet.GetMsgNum());
        }

        [Fact]
        public void GetMsg_InvalidIndex_ReturnsNull()
        {
            COPSPacket packet = CreatePacket();
            packet.Parse(strIn);

            Assert.Null(packet.GetMsg(5));
        }
       
    }
}
