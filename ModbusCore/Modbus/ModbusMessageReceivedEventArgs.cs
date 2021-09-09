using System;
using ModbusCore.Devices;

namespace ModbusCore
{
    public class ModbusMessageReceivedEventArgs
    {
        public IModbusMessage Message { get; }
        public ModbusMessageType Type { get; }

        public ModbusMessageReceivedEventArgs(IModbusMessage message, ModbusMessageType type)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Type = type;
        }
    }
}
