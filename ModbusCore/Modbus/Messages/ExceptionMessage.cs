using System;

namespace ModbusCore.Messages
{
    /// <summary>
    /// Modbus message representing response for exception function codes (above 0x80).
    /// </summary>
    public record ExceptionMessage : MessageBase
    {
        public ModbusFunctionCode OriginalFunction { get; }
        public ModbusExceptionCode ExceptionCode { get; init; }

        public ExceptionMessage() : base(ModbusMessageType.Response) { }

        public ExceptionMessage(ReadOnlySpan<byte> buffer)
            : base(buffer, ModbusMessageType.Response)
        {
            ValidateBufferLength(buffer, 3);

            OriginalFunction = ModbusUtility.GetFunctionCodeFromException(Function);
            ExceptionCode = (ModbusExceptionCode)buffer[2];
        }

        public override bool TryWriteTo(Span<byte> buffer, out int length)
        {
            base.TryWriteTo(buffer, out length);
            length += 1;

            if (buffer.Length < length)
                return false;

            buffer[2] = (byte)ExceptionCode;
            return true;
        }
    }
}
