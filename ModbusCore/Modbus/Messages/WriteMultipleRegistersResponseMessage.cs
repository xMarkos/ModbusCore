using System;

namespace ModbusCore.Messages;

/// <summary>
/// Modbus message representing response for function codes:
/// <list type="bullet">
/// <item><see cref="ModbusFunctionCode.WriteMultipleHoldingRegisters"/></item>
/// </list>
/// </summary>
public record WriteMultipleRegistersResponseMessage : MessageBase
{
    public ushort Register { get; init; }
    public ushort Count { get; init; }

    public WriteMultipleRegistersResponseMessage() : base(ModbusMessageType.Response) { }

    public WriteMultipleRegistersResponseMessage(ReadOnlySpan<byte> buffer)
        : base(buffer, ModbusMessageType.Response)
    {
        ValidateBufferLength(buffer, 6);

        Register = ModbusUtility.ReadUInt16(buffer[2..]);
        Count = ModbusUtility.ReadUInt16(buffer[4..]);
    }
}
