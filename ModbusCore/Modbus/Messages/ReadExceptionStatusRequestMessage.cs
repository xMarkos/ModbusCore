using System;

namespace ModbusCore.Messages;

/// <summary>
/// Modbus message representing request for function codes:
/// <list type="bullet">
/// <item><see cref="ModbusFunctionCode.ReadExceptionStatus"/></item>
/// </list>
/// </summary>
public record ReadExceptionStatusRequestMessage : MessageBase
{
    public ReadExceptionStatusRequestMessage() : base(ModbusMessageType.Request) { }
    public ReadExceptionStatusRequestMessage(ReadOnlySpan<byte> buffer) : base(buffer, ModbusMessageType.Request) { }
}
