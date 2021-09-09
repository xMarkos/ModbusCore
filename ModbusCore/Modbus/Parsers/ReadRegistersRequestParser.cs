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
            if (buffer.Length < 2)
                throw new ArgumentException("Input buffer is too short", nameof(buffer));

            /*
             1: address of slave
             1: function
             2: start register (register num - 1)
             2: count of registers
             2: CRC16-IBM
             */

            byte function = buffer[1];
            if (!CanHandle(function, type))
                throw new NotSupportedException();

            if (!TryGetFrameLength(buffer, type, out int length) || buffer.Length < length)
                throw new ArgumentException("Input buffer is too short", nameof(buffer));

            return new ReadRegistersRequestMessage(buffer[..6].ToArray());
        }
    }
}
