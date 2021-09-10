using System;
using ModbusCore.Messages;

namespace ModbusCore.Parsers
{
    public class ReadRegistersRequestParser : IMessageParser
    {
        public bool CanHandle(byte function, ModbusMessageType type)
            => type == ModbusMessageType.Request && function is 3 or 4;

        public bool TryGetFrameLength(ReadOnlySpan<byte> buffer, ModbusMessageType type, out int length)
        {
            length = 6;
            return true;
        }

        public IModbusMessage Parse(ReadOnlySpan<byte> buffer, ModbusMessageType type)
        {
            /*
             1: address of slave
             1: function
             2: start register (register num - 1)
             2: count of registers
             */

            this.ValidateParse(buffer, type);
            return new ReadRegistersRequestMessage(buffer[..6].ToArray());
        }
    }
}
