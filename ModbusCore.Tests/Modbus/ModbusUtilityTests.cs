using Xunit;

namespace ModbusCore
{
    public class ModbusUtilityTests
    {
        [Theory]
        [InlineData(new byte[] { 0x11, 0x03, 0x00, 0x6B, 0x00, 0x03 }, 0x8776)]
        [InlineData(new byte[] { 1, 4, 0, 101, 0, 4 }, 0xE1D6)]
        [InlineData(new byte[] { 0x01, 0x04, 0x02, 0xFF, 0xFF }, 0xD880)]
        public void CalculateCrc16_ReturnsCorrectChecksum(byte[] input, ushort expected)
        {
            // Act
            ushort actual = ModbusUtility.CalculateCrc16(input);

            // Verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(new byte[] { 0x04, 0x03, 0x02, 0x01 }, 0x04030201u)]
        [InlineData(new byte[] { 0x04, 0x03, 0x02, 0x01, 0x99 }, 0x04030201u)]
        public void ReadUInt32_ReadsValueWithCorrectEndianness(byte[] input, uint expected)
        {
            // Act
            ulong actual = ModbusUtility.ReadUInt32(input);

            // Verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(new byte[] { 0x01, 0x00 }, 256u)]
        public void ReadUInt16_ReadsValueWithCorrectEndianness(byte[] input, uint expected)
        {
            // Act
            ulong actual = ModbusUtility.ReadUInt16(input);

            // Verify
            Assert.Equal(expected, actual);
        }
    }
}
