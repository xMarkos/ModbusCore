using System;
using System.Text;

namespace ModbusCore.Messages
{
    /// <summary>
    /// Modbus message representing request for function codes:
    /// <list type="bullet">
    /// <item><see cref="ModbusFunctionCode.ReadWriteMultipleRegisters"/></item>
    /// </list>
    /// </summary>
    public record ReadWriteMultipleRegistersRequestMessage : MessageBase
    {
        public ushort ReadRegister { get; init; }
        public ushort ReadCount { get; init; }
        public ushort WriteRegister { get; init; }
        public ushort WriteCount => checked((byte)WriteData.Length);
        public byte WriteDataLength => checked((byte)(WriteData.Length * 2));

        private readonly short[] _writeData = null!;
        public short[] WriteData
        {
            get => _writeData;
            init => _writeData = value ?? Array.Empty<short>();
        }

        public ReadWriteMultipleRegistersRequestMessage()
            => WriteData = null!;

        public ReadWriteMultipleRegistersRequestMessage(ReadOnlySpan<byte> buffer)
            : base(buffer)
        {
            ValidateBufferLength(buffer, 11);

            ReadRegister = ModbusUtility.ReadUInt16(buffer[2..]);
            ReadCount = ModbusUtility.ReadUInt16(buffer[4..]);

            WriteRegister = ModbusUtility.ReadUInt16(buffer[6..]);
            ushort count = ModbusUtility.ReadUInt16(buffer[8..]);

            byte writeLength = buffer[10];
            if (writeLength % 2 != 0)
                throw new FormatException("WriteDataLength is not multiple of 2");

            if (writeLength != count * 2)
                throw new ArgumentException("The WriteCount and WriteDataLength does not match", nameof(buffer));

            if (buffer.Length < writeLength + 11)
                throw new FormatException("Unexpected end of data");

            WriteData = new short[count];
            ModbusUtility.ReadRegisters(buffer[11..], WriteData.Length, WriteData);
        }

        public override bool TryWriteTo(Span<byte> buffer, out int length)
        {
            base.TryWriteTo(buffer, out length);
            length += 9 + WriteDataLength;

            if (buffer.Length < length)
                return false;

            ModbusUtility.Write(buffer[2..], ReadRegister);
            ModbusUtility.Write(buffer[4..], ReadCount);
            ModbusUtility.Write(buffer[6..], WriteRegister);
            ModbusUtility.Write(buffer[8..], WriteCount);
            ModbusUtility.Write(buffer[10..], WriteDataLength);
            ModbusUtility.WriteRegisters(buffer[11..], WriteData, WriteCount);
            return true;
        }

        protected override bool PrintMembers(StringBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            if (base.PrintMembers(builder))
                builder.Append(", ");

            builder.AppendFormat("{0} = {1}, ", nameof(ReadRegister), ReadRegister);
            builder.AppendFormat("{0} = {1}, ", nameof(ReadCount), ReadCount);
            builder.AppendFormat("{0} = {1}, ", nameof(WriteRegister), WriteRegister);
            builder.AppendFormat("{0} = {1}, ", nameof(WriteCount), WriteCount);
            builder.AppendFormat("{0} = {1}", nameof(WriteDataLength), WriteDataLength);

            return true;
        }
    }

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
            init => _data = value ?? Array.Empty<short>();
        }

        public WriteMultipleRegistersRequestMessage() { }

        public WriteMultipleRegistersRequestMessage(ReadOnlySpan<byte> buffer)
            : base(buffer)
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
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            if (base.PrintMembers(builder))
                builder.Append(", ");

            builder.AppendFormat("{0} = {1}, ", nameof(Register), Register);
            builder.AppendFormat("{0} = {1}, ", nameof(Count), Count);
            builder.AppendFormat("{0} = {1}", nameof(DataLength), DataLength);

            return true;
        }
    }
}
