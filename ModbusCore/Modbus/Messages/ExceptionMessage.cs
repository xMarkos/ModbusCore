using System;

namespace ModbusCore.Messages
{
    public record ExceptionMessage : MessageBase
    {
        public byte ExceptionCode { get; init; }

        public ExceptionMessage() { }

        public ExceptionMessage(ReadOnlySpan<byte> buffer)
            : base(buffer)
        {
            if (buffer.Length < 3)
                throw new ArgumentException(null, nameof(buffer));

            ExceptionCode = buffer[2];
        }

        public override bool TryWriteTo(Span<byte> buffer, out int length)
        {
            base.TryWriteTo(buffer, out length);
            length += 1;

            if (buffer.Length < length)
                return false;

            buffer[2] = ExceptionCode;
            return true;
        }
    }
}
