using System;
using System.Text;

namespace ModbusCore.Messages
{
    public record RawModbusMessage(byte[] Buffer) : IModbusMessage
    {
        public byte Address => Buffer[0];
        public ModbusFunctionCode Function => (ModbusFunctionCode)Buffer[1];

        public bool TryWriteTo(Span<byte> buffer, out int length)
        {
            length = Buffer.Length;

            if (buffer.Length < Buffer.Length)
                return false;

            Buffer.AsSpan().CopyTo(buffer);
            return true;
        }

        protected virtual bool PrintMembers(StringBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.AppendFormat("A = {1}, ", nameof(Address), Address);
            builder.AppendFormat("F = {1}, ", nameof(Function), Function);
            builder.AppendFormat("Data Length = {0}, ", Buffer.Length - 2);

            return true;
        }
    }
}
