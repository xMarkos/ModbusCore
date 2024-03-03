using System;
using System.Text;

namespace ModbusCore.Messages;

/// <summary>
/// Modbus message representing request and response for function codes:
/// <list type="bullet">
/// <item><see cref="ModbusFunctionCode.WriteSingleCoil"/></item>
/// <item><see cref="ModbusFunctionCode.WriteSingleHoldingRegister"/></item>
/// </list>
/// </summary>
public record WriteSingleValueMessage : MessageBase
{
    public ushort Register { get; init; }
    public ushort Value { get; init; }

    public bool CoilValue
    {
        // By specification, only 0xFF00 is true, and 0x0000 is false, but let's parse any non-zero value as true
        get => Value != 0;
        init => Value = (ushort)(value ? 0xFF00 : 0x0000);
    }

    public WriteSingleValueMessage() { }

    public WriteSingleValueMessage(ReadOnlySpan<byte> buffer, ModbusMessageType type)
        : base(buffer, type)
    {
        ValidateBufferLength(buffer, 6);

        Register = ModbusUtility.ReadUInt16(buffer[2..]);
        Value = ModbusUtility.ReadUInt16(buffer[4..]);
    }

    public override bool TryWriteTo(Span<byte> buffer, out int length)
    {
        base.TryWriteTo(buffer, out length);
        length += 4;

        if (buffer.Length < length)
            return false;

        ModbusUtility.Write(buffer[2..], Register);
        ModbusUtility.Write(buffer[4..], Value);
        return true;
    }

    protected override bool PrintMembers(StringBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (base.PrintMembers(builder))
            builder.Append(", ");

        if (Function == ModbusFunctionCode.WriteSingleCoil)
        {
            builder.AppendFormat("{0} = {1}", nameof(CoilValue), CoilValue);
        }
        else
        {
            builder.AppendFormat("{0} = {1}", nameof(Value), Value);
        }

        return true;
    }
}
