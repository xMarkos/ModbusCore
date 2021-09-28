using System;

namespace ModbusCore.Messages
{
    public record ReadExceptionStatusResponseMessage : MessageBase
    {
        public byte Value { get; init; }

        public ReadExceptionStatusResponseMessage() { }

        public ReadExceptionStatusResponseMessage(ReadOnlySpan<byte> buffer)
            : base(buffer)
        {
            ValidateBufferLength(buffer, 3);

            Value = buffer[2];
        }

        public override bool TryWriteTo(Span<byte> buffer, out int length)
        {
            base.TryWriteTo(buffer, out length);
            length += 1;

            if (buffer.Length < length)
                return false;

            buffer[2] = Value;
            return true;
        }
    }
}
