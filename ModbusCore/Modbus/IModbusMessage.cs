using System;

namespace ModbusCore
{
    public interface IModbusMessage
    {
        public byte Address { get; }
        public ModbusFunctionCode Function { get; }

        public bool TryWriteTo(Span<byte> buffer, out int length);
    }
}
