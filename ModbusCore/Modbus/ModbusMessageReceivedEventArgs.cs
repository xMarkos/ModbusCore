using System;

namespace ModbusCore
{
    public class ModbusMessageReceivedEventArgs
    {
        public IModbusMessage Message { get; }
        public ModbusMessageType Type { get; }
        public ModbusMessagePriority Priority { get; }

        public ModbusMessageReceivedEventArgs(IModbusMessage message, ModbusMessageType type, ModbusMessagePriority priority)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Type = type;
            Priority = priority;
        }
    }
}
