using System;
using ModbusCore.Messages;

namespace ModbusCore.Parsers;

public class ReadWriteMultipleRegistersRequestMessageParser : IMessageParser
{
    public bool CanHandle(ReadOnlySpan<byte> buffer, ModbusMessageType type)
        => type is ModbusMessageType.Request && (ModbusFunctionCode)buffer[1] is ModbusFunctionCode.ReadWriteMultipleRegisters;

    public bool TryGetFrameLength(ReadOnlySpan<byte> buffer, ModbusMessageType type, out int length)
    {
        if (buffer.Length < 11)
        {
            length = 11;
            return false;
        }

        length = buffer[10] + 11;
        return true;
    }

    public IModbusMessage Parse(ReadOnlySpan<byte> buffer, ModbusMessageType type)
    {
        int length = this.ValidateParse(buffer, type);

        /*
         1: address of slave
         1: function
         2: start read register (register num - 1)
         2: count of read registers
         2: start write register (register num - 1)
         2: count of write registers
         1: length of write data (count of write registers * 2)
         n: data
         */

        return new ReadWriteMultipleRegistersRequestMessage(buffer[..length]);
    }
}
