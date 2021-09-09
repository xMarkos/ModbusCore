using System;
using ModbusCore.Messages;
using Xunit;

namespace ModbusCore.Parsers
{
    public class ReadRegistersRequestParserTests
    {
        [Fact]
        public void ReadRegistersRequestParser_ParsesRequestCorrectly()
        {
            // Arrange
            ReadRegistersRequestParser target = new();
            ReadOnlySpan<byte> input = new byte[] { 0x11, 0x03, 0x00, 0x6B, 0x00, 0x04, 0x76, 0x87 };

            // Act
            IModbusMessage actual = target.Parse(input, ModbusMessageType.Request);

            // Verify
            ReadRegistersRequestMessage message = Assert.IsType<ReadRegistersRequestMessage>(actual);

            Assert.Equal(0x11, message.Address);
            Assert.Equal(0x03, message.Function);
            Assert.Equal(0x6B, message.Register);
            Assert.Equal(0x04, message.Count);
        }
    }
}
