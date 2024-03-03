using System;
using System.Text;

namespace ModbusCore.Messages;

/// <summary>
/// Modbus message representing request for function codes:
/// <list type="bullet">
/// <item><see cref="ModbusFunctionCode.WriteMultipleHoldingRegisters"/></item>
/// </list>
/// </summary>
public record WriteMultipleRegistersRequestMessage : MessageBase
{
    public ushort Register { get; init; }

    public ushort Count => checked((byte)Data.Length);
    public byte DataLength => checked((byte)(Data.Length * 2));

    private readonly short[] _data = null!;
    public short[] Data
    {
        get => _data;
        init => _data = value ?? [];
    }

    public WriteMultipleRegistersRequestMessage() : base(ModbusMessageType.Request) { }

    public WriteMultipleRegistersRequestMessage(ReadOnlySpan<byte> buffer)
        : base(buffer, ModbusMessageType.Request)
    {
        if (buffer.Length < 7)
            throw new ArgumentException("The buffer must be at least 7 bytes long", nameof(buffer));

        Register = ModbusUtility.ReadUInt16(buffer[2..]);

        ushort count = ModbusUtility.ReadUInt16(buffer[4..]);

        byte length = buffer[6];
        if (length % 2 != 0)
            throw new FormatException("DataLength is not multiple of 2");

        if (length != count * 2)
            throw new ArgumentException("The Count and DataLength does not match", nameof(buffer));

        if (buffer.Length < length + 7)
            throw new FormatException("Unexpected end of data");

        Data = new short[count];
        ModbusUtility.ReadRegisters(buffer[7..], Data.Length, Data);
    }

    public override bool TryWriteTo(Span<byte> buffer, out int length)
    {
        base.TryWriteTo(buffer, out length);
        length += 5 + DataLength;

        if (buffer.Length < length)
            return false;

        ModbusUtility.Write(buffer[2..], Register);
        ModbusUtility.Write(buffer[4..], Count);
        ModbusUtility.Write(buffer[6..], DataLength);
        ModbusUtility.WriteRegisters(buffer[7..], Data, Count);
        return true;
    }

    protected override bool PrintMembers(StringBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (base.PrintMembers(builder))
            builder.Append(", ");

        builder.AppendFormat("{0} = {1}, ", nameof(Register), Register);
        builder.AppendFormat("{0} = {1}, ", nameof(Count), Count);
        builder.AppendFormat("{0} = {1}", nameof(DataLength), DataLength);

        return true;
    }
}
