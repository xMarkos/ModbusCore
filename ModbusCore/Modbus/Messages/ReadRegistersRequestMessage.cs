using System;

namespace ModbusCore.Messages
{
    /// <summary>
    /// Modbus message representing request for function codes:
    /// <list type="bullet">
    /// <item><see cref="ModbusFunctionCode.ReadCoils"/></item>
    /// <item><see cref="ModbusFunctionCode.ReadDiscreteInputs"/></item>
    /// <item><see cref="ModbusFunctionCode.ReadHoldingRegisters"/></item>
    /// <item><see cref="ModbusFunctionCode.ReadInputRegisters"/></item>
    /// </list>
    /// </summary>
    public record ReadRegistersRequestMessage : MessageBase
    {
        public ushort Register { get; init; }
        public ushort Count { get; init; }

        public ReadRegistersRequestMessage() : base(ModbusMessageType.Request) { }

        public ReadRegistersRequestMessage(ReadOnlySpan<byte> buffer)
            : base(buffer, ModbusMessageType.Request)
        {
            ValidateBufferLength(buffer, 6);

            Register = ModbusUtility.ReadUInt16(buffer[2..]);
            Count = ModbusUtility.ReadUInt16(buffer[4..]);
        }

        public override bool TryWriteTo(Span<byte> buffer, out int length)
        {
            base.TryWriteTo(buffer, out length);
            length += 4;

            if (buffer.Length < length)
                return false;

            ModbusUtility.Write(buffer[2..], Register);
            ModbusUtility.Write(buffer[4..], Count);
            return true;
        }
    }
}
