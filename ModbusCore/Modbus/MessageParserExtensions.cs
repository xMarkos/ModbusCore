using System;

namespace ModbusCore
{
    internal static class MessageParserExtensions
    {
        public static int ValidateParse(this IMessageParser parser, ReadOnlySpan<byte> buffer, ModbusMessageType type)
        {
            if (buffer.Length < 2)
                throw new ArgumentException(null, nameof(buffer));

            if (!parser.CanHandle(buffer[1], type))
                throw new NotSupportedException();

            if (!parser.TryGetFrameLength(buffer, type, out int length) || buffer.Length < length)
                throw new ArgumentException(null, nameof(buffer));

            return length;
        }

        public static bool CanHandle(this IMessageParser parser, byte function, ModbusMessageType type)
            => parser.CanHandle((ModbusFunctionCode)function, type);
    }
}
