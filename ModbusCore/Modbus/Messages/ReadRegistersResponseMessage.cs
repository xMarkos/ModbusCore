using System;
using System.Text;

namespace ModbusCore.Messages;

/// <summary>
/// Modbus message representing response for function codes:
/// <list type="bullet">
/// <item><see cref="ModbusFunctionCode.ReadHoldingRegisters"/></item>
/// <item><see cref="ModbusFunctionCode.ReadInputRegisters"/></item>
/// <item><see cref="ModbusFunctionCode.ReadWriteMultipleRegisters"/></item>
/// </list>
/// </summary>
public record ReadRegistersResponseMessage : MessageBase
{
    public byte DataLength => checked((byte)(Data.Length * 2));

    private readonly short[] _data = null!;
    public short[] Data
    {
        get => _data;
        init => _data = value ?? [];
    }

    public ReadRegistersResponseMessage() : base(ModbusMessageType.Response)
        => Data = null!;

    public ReadRegistersResponseMessage(ReadOnlySpan<byte> buffer)
        : base(buffer, ModbusMessageType.Response)
    {
        ValidateBufferLength(buffer, 3);

        int length = buffer[2];
        if (length % 2 != 0)
            throw new FormatException("Length is not multiple of 2");

        ValidateBufferLength(buffer, length + 3);

        Data = new short[length / 2];
        ModbusUtility.ReadRegisters(buffer[3..], Data.Length, Data);
    }

    public override bool TryWriteTo(Span<byte> buffer, out int length)
    {
        base.TryWriteTo(buffer, out length);
        length += DataLength + 1;

        if (buffer.Length < length)
            return false;

        buffer[2] = DataLength;

        if (Data.Length > 0)
            ModbusUtility.WriteRegisters(buffer[3..], Data, Data.Length);

        return true;
    }

    protected override bool PrintMembers(StringBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (base.PrintMembers(builder))
            builder.Append(", ");

        builder.AppendFormat("{0} = {1}", nameof(DataLength), DataLength);

        return true;
    }
}
