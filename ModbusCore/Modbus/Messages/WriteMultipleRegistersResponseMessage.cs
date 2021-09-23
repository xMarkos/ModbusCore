using System;

namespace ModbusCore.Messages
{
    /// <summary>
    /// Modbus message representing response for function codes:
    /// <list type="bullet">
    /// <item><see cref="ModbusFunctionCode.WriteMultipleHoldingRegisters"/></item>
    /// </list>
    /// </summary>
    public record WriteMultipleRegistersResponseMessage : MessageBase
    {
        public ushort Register { get; init; }
        public ushort Count { get; init; }

        public WriteMultipleRegistersResponseMessage() { }

        public WriteMultipleRegistersResponseMessage(ReadOnlySpan<byte> buffer)
            : base(buffer)
        {
            if (buffer.Length < 6)
                throw new ArgumentException(null, nameof(buffer));

            Register = ModbusUtility.ReadUInt16(buffer[2..]);
            Count = ModbusUtility.ReadUInt16(buffer[4..]);
        }
    }
}
