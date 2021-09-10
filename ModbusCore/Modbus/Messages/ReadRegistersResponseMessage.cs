using System;

namespace ModbusCore.Messages
{
    /// <summary>
    /// Modbus message representing response for function codes:
    /// <list type="bullet">
    /// <item><see cref="ModbusFunctionCode.ReadHoldingRegisters"/></item>
    /// <item><see cref="ModbusFunctionCode.ReadInputRegisters"/></item>
    /// </list>
    /// </summary>
    public record ReadRegistersResponseMessage : MessageBase
    {
        public byte DataLength => checked((byte)((Data?.Length ?? 0) * 2));
        public short[]? Data { get; init; }

        public ReadRegistersResponseMessage() { }

        public ReadRegistersResponseMessage(ReadOnlySpan<byte> buffer)
            : base(buffer)
        {
            if (buffer.Length < 3)
                throw new FormatException("Unexpected end of data");

            int length = buffer[2];
            if (length % 2 != 0)
                throw new FormatException("Length is not multiple of 2");

            if (buffer.Length < length + 3)
                throw new FormatException("Unexpected end of data");

            Data = new short[length / 2];
            ModbusUtility.ReadRegisters(buffer[3..], Data.Length, Data);
        }

        public override bool TryWriteTo(Span<byte> buffer, out int length)
        {
            base.TryWriteTo(buffer, out length);
            length += DataLength + 1;

            if (buffer.Length < length)
                return false;

            ModbusUtility.Write(buffer[2..], DataLength);

            if (Data?.Length > 0)
                ModbusUtility.WriteRegisters(buffer[3..], Data, Data.Length);

            return true;
        }
    }
}
