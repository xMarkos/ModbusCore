using System;

namespace ModbusCore.Messages
{
    public record ReadRegistersRequestMessage : MessageBase
    {
        public ushort Register { get; init; }
        public ushort Count { get; init; }

        public ReadRegistersRequestMessage() { }

        public ReadRegistersRequestMessage(ReadOnlySpan<byte> buffer)
            : base(buffer)
        {
            if (buffer.Length < 6)
                throw new ArgumentException(null, nameof(buffer));

            Register = ModbusUtility.ReadUInt16(buffer[2..]);
            Count = ModbusUtility.ReadUInt16(buffer[4..]);
        }
    }
}
