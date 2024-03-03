using System;

namespace ModbusCore;

public class ModbusMessageReceivedEventArgs(IModbusMessage message)
{
    public IModbusMessage Message { get; } = message ?? throw new ArgumentNullException(nameof(message));
}
