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
    }
}
