using System;

namespace ModbusCore.Messages
{
    public record MessageBase : IModbusMessage
    {
        public byte Address { get; init; }
        public byte Function { get; init; }

        public MessageBase() { }

        public MessageBase(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < 2)
                throw new ArgumentException(null, nameof(buffer));

            Address = buffer[0];
            Function = buffer[1];
        }

        public virtual bool TryWriteTo(Span<byte> buffer, out int length)
        {
            length = 2;

            if (buffer.Length < length)
                return false;

            buffer[0] = Address;
            buffer[1] = Function;
            return true;
        }
    }
}
