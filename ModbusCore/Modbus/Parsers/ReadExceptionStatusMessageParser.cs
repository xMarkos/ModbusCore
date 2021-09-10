using System;
using ModbusCore.Messages;

namespace ModbusCore.Parsers
{
    public class ReadExceptionStatusMessageParser : IMessageParser
    {
        public bool CanHandle(ModbusFunctionCode function, ModbusMessageType type)
            => function == ModbusFunctionCode.ReadExceptionStatus;

        public bool TryGetFrameLength(ReadOnlySpan<byte> buffer, ModbusMessageType type, out int length)
        {
            length = type switch
            {
                ModbusMessageType.Request => 2,
                ModbusMessageType.Response => 3,
                _ => throw new NotSupportedException(),
            };

            return true;
        }

        public IModbusMessage Parse(ReadOnlySpan<byte> buffer, ModbusMessageType type)
        {
            int length = this.ValidateParse(buffer, type);

            return type switch
            {
                ModbusMessageType.Request => new ReadExceptionStatusRequestMessage(buffer[..length]),
                ModbusMessageType.Response => new ReadExceptionStatusResponseMessage(buffer[..length]),
                _ => throw new NotSupportedException(),
            };
        }
    }
}
