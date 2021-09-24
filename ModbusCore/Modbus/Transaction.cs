using System;

namespace ModbusCore
{
    public record Transaction(byte Address, ModbusFunctionCode Function)
    {
        public static Transaction From(IModbusMessage message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            return new(message.Address, ModbusUtility.GetFunctionCodeFromException(message.Function));
        }
    }
}
