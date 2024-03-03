using System;
using ModbusCore.Messages;

namespace ModbusCore.Parsers;

public class ReadCoilsResponseMessageParser : IMessageParser
{
    public bool CanHandle(ReadOnlySpan<byte> buffer, ModbusMessageType type)
        => type == ModbusMessageType.Response && (ModbusFunctionCode)buffer[1] is ModbusFunctionCode.ReadCoils or ModbusFunctionCode.ReadDiscreteInputs;

    public bool TryGetFrameLength(ReadOnlySpan<byte> buffer, ModbusMessageType type, out int length)
    {
        if (buffer.Length < 3)
        {
            length = 3;
            return false;
        }

        length = buffer[2] + 3;
        return true;
    }

    public IModbusMessage Parse(ReadOnlySpan<byte> buffer, ModbusMessageType type)
    {
        int length = this.ValidateParse(buffer, type);
        return new ReadCoilsResponseMessage(buffer[..length]);
    }
}
