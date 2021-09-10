using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ModbusCore
{
    internal static class ParserRegistryExtensions
    {
        public static bool TryGetParser(this IEnumerable<IMessageParser> registry, ReadOnlySpan<byte> buffer, ModbusMessageType type, [NotNullWhen(true)] out IMessageParser? result)
        {
            if (registry is null)
                throw new ArgumentNullException(nameof(registry));

            byte function = buffer[1];

            foreach (IMessageParser parser in registry)
            {
                if (parser.CanHandle(function, type))
                {
                    result = parser;
                    return true;
                }
            }

            result = null;
            return false;
        }

        public static IMessageParser GetParser(this IEnumerable<IMessageParser> registry, ReadOnlySpan<byte> buffer, ModbusMessageType type)
        {
            if (!registry.TryGetParser(buffer, type, out IMessageParser? parser))
                throw new NotSupportedException($"Message type={type} function={buffer[1]} is not supported");

            return parser;
        }
    }
}
