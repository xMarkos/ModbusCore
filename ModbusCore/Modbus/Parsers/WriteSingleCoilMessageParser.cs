using System;
using ModbusCore.Messages;

namespace ModbusCore.Parsers
{
    public class WriteSingleCoilMessageParser : IMessageParser
    {
        public bool CanHandle(ModbusFunctionCode function, ModbusMessageType type)
            => type is ModbusMessageType.Request or ModbusMessageType.Response && function is ModbusFunctionCode.WriteSingleCoil;

        public bool TryGetFrameLength(ReadOnlySpan<byte> buffer, ModbusMessageType type, out int length)
        {
            length = 6;
            return true;
        }

        public IModbusMessage Parse(ReadOnlySpan<byte> buffer, ModbusMessageType type)
        {
            int length = this.ValidateParse(buffer, type);
            return new WriteSingleCoilMessage(buffer[..length]);
        }
    }
}
