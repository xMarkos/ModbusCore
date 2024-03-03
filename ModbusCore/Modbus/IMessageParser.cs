using System;

namespace ModbusCore
{
    public interface IMessageParser
    {
        bool CanHandle(ReadOnlySpan<byte> buffer, ModbusMessageType type);
        bool TryGetFrameLength(ReadOnlySpan<byte> buffer, ModbusMessageType type, out int length);
        IModbusMessage Parse(ReadOnlySpan<byte> buffer, ModbusMessageType type);
    }
}
