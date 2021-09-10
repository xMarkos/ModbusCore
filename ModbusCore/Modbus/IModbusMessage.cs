using System;

namespace ModbusCore
{
    public interface IModbusMessage
    {
        public byte Address { get; }
        public byte Function { get; }

        public int WriteTo(Span<byte> buffer);
    }
}
