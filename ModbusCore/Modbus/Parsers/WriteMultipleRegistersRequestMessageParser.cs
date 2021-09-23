using System;
using ModbusCore.Messages;

namespace ModbusCore.Parsers
{
    public class WriteMultipleRegistersRequestMessageParser : IMessageParser
    {
        public bool CanHandle(ModbusFunctionCode function, ModbusMessageType type)
            => type is ModbusMessageType.Request && function is ModbusFunctionCode.WriteMultipleHoldingRegisters;

        public bool TryGetFrameLength(ReadOnlySpan<byte> buffer, ModbusMessageType type, out int length)
        {
            if (buffer.Length < 7)
            {
                length = 7;
                return false;
            }

            length = buffer[6] + 7;
            return true;
        }

        public IModbusMessage Parse(ReadOnlySpan<byte> buffer, ModbusMessageType type)
        {
            int length = this.ValidateParse(buffer, type);
            return new WriteMultipleRegistersRequestMessage(buffer[..length]);
        }
    }
}
