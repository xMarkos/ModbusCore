using System;
using System.Collections;
using System.Text;

namespace ModbusCore.Messages
{
    /// <summary>
    /// Modbus message representing response for function codes:
    /// <list type="bullet">
    /// <item><see cref="ModbusFunctionCode.ReadCoils"/></item>
    /// <item><see cref="ModbusFunctionCode.ReadDiscreteInputs"/></item>
    /// </list>
    /// </summary>
    public record ReadCoilsResponseMessage : MessageBase
    {
        public byte DataLength => checked((byte)_data.Length);

        private readonly byte[] _data = null!;
        public byte[] Data
        {
            get => _data;
            init
            {
                _data = value ?? Array.Empty<byte>();
                Bits = new(_data);
            }
        }

        public BitArray Bits { get; private set; } = null!;

        public ReadCoilsResponseMessage() : base(ModbusMessageType.Response)
            => Data = null!;

        public ReadCoilsResponseMessage(ReadOnlySpan<byte> buffer)
            : base(buffer, ModbusMessageType.Response)
        {
            ValidateBufferLength(buffer, 3);

            int length = buffer[2];

            ValidateBufferLength(buffer, length + 3);

            Data = buffer.Slice(3, length).ToArray();

            /* Read coils
             * byte order: big endian
             * bit order: little endian
             * that means if we query 14 bits we get 2 bytes, where the order is not entirely natural:
             *  1st: bits 0-7   07|06|05|04|03|02|01|00
             *  2nd: bits 8-13  --|--|13|12|11|10|09|08
             * 
             * Luckily this is exactly how BitArray works.
             */
        }

        public override bool TryWriteTo(Span<byte> buffer, out int length)
        {
            base.TryWriteTo(buffer, out length);
            length += DataLength + 1;

            if (buffer.Length < length)
                return false;

            buffer[2] = DataLength;
            Data.AsSpan().CopyTo(buffer[3..]);
            return true;
        }

        protected override bool PrintMembers(StringBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            if (base.PrintMembers(builder))
                builder.Append(", ");

            builder.AppendFormat("{0} = {1}", nameof(DataLength), DataLength);

            return true;
        }
    }
}
