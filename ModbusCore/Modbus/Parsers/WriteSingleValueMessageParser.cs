using System;
using ModbusCore.Messages;

namespace ModbusCore.Parsers
{
    public class WriteSingleValueMessageParser : IMessageParser
    {
        public bool CanHandle(ModbusFunctionCode function, ModbusMessageType type)
            => type is ModbusMessageType.Request or ModbusMessageType.Response && function is ModbusFunctionCode.WriteSingleCoil or ModbusFunctionCode.WriteSingleHoldingRegister;

        public bool TryGetFrameLength(ReadOnlySpan<byte> buffer, ModbusMessageType type, out int length)
        {
            length = 6;
            return true;
        }

        public IModbusMessage Parse(ReadOnlySpan<byte> buffer, ModbusMessageType type)
        {
            int length = this.ValidateParse(buffer, type);
            return new WriteSingleValueMessage(buffer[..length]);
        }
    }
}
