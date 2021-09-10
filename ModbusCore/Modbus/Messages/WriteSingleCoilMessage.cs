using System;

namespace ModbusCore.Messages
{
    public record WriteSingleCoilMessage : MessageBase
    {
        public ushort Register { get; init; }
        public bool Value { get; init; }

        public WriteSingleCoilMessage() { }

        public WriteSingleCoilMessage(ReadOnlySpan<byte> buffer)
            : base(buffer)
        {
            if (buffer.Length < 6)
                throw new ArgumentException(null, nameof(buffer));

            Register = ModbusUtility.ReadUInt16(buffer[2..]);

            // By specification, only 0xFF00 is true, and 0x0000 is false, but let's parse as non-zero value as true
            Value = ModbusUtility.ReadUInt16(buffer[4..]) != 0;
        }

        public override bool TryWriteTo(Span<byte> buffer, out int length)
        {
            base.TryWriteTo(buffer, out length);
            length += 4;

            if (buffer.Length < length)
                return false;

            ModbusUtility.Write(buffer[2..], Register);
            ModbusUtility.Write(buffer[4..], Value ? 0xFF00 : 0x0000);
            return true;
        }
    }
}
