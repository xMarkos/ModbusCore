using System;
using ModbusCore.Messages;

namespace ModbusCore.Parsers
{
    public class WriteSingleCoilRequestMessageParser : IMessageParser
    {
        public bool CanHandle(ModbusFunctionCode function, ModbusMessageType type)
            => type == ModbusMessageType.Request && function is ModbusFunctionCode.WriteSingleCoil;

        public bool TryGetFrameLength(ReadOnlySpan<byte> buffer, ModbusMessageType type, out int length)
        {
            length = 6;
            return true;
        }

        public IModbusMessage Parse(ReadOnlySpan<byte> buffer, ModbusMessageType type)
        {
            int length = this.ValidateParse(buffer, type);
            return new WriteSingleCoilRequestMessage(buffer[..length]);
        }
    }
}
