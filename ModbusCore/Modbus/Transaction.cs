using System;

namespace ModbusCore;

public record Transaction(byte Address, ModbusFunctionCode Function)
{
    public static Transaction From(IModbusMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return new(message.Address, ModbusUtility.GetFunctionCodeFromException(message.Function));
    }
}
