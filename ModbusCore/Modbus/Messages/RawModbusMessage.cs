using System;
using System.Text;

namespace ModbusCore.Messages
{
    /// <summary>
    /// Modbus message representing raw, mutable, and unchecked message frame.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// The buffer is taken as is and used directly as the frame data, i.e. modifying the data in the buffer
    /// after the message is constructed also changes the meaning of the message.
    /// </item>
    /// <item>
    /// The data in the buffer is not checked and may be completely invalid, however, it allows the user
    /// to send messages that are not available in the implemented types.
    /// </item>
    /// <item>
    /// Copying the messages also clones the buffer, making the 2 messages to be independent.
    /// </item>
    /// </list>
    /// </remarks>
    public record RawModbusMessage(byte[] Buffer, ModbusMessageType Type) : IModbusMessage
    {
        public ModbusMessageType Type { get; set; } = Type;

        public byte Address => Buffer[0];
        public ModbusFunctionCode Function => (ModbusFunctionCode)Buffer[1];

        protected RawModbusMessage(RawModbusMessage original)
        {
            if (original is null)
                throw new ArgumentNullException(nameof(original));

            Type = original.Type;
            Buffer = (byte[])original.Buffer.Clone();
        }

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

            builder.AppendFormat("T = {0}, ", Type);
            builder.AppendFormat("A = {0}, ", Address);
            builder.AppendFormat("F = {0}, ", Function);
            builder.AppendFormat("Data Length = {0}, ", Buffer.Length - 2);

            return true;
        }
    }
}
