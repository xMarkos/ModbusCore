using System;

namespace ModbusCore
{
    public class ModbusMessageReceivedEventArgs
    {
        public IModbusMessage Message { get; }

        public ModbusMessageReceivedEventArgs(IModbusMessage message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
}
