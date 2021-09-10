﻿using System;
using ModbusCore.Messages;

namespace ModbusCore.Parsers
{
    public class ExceptionMessageParser : IMessageParser
    {
        public bool CanHandle(ModbusFunctionCode function, ModbusMessageType type)
            => ((byte)function & 0b1000_0000) != 0;

        public bool TryGetFrameLength(ReadOnlySpan<byte> buffer, ModbusMessageType type, out int length)
        {
            length = 3;
            return true;
        }

        public IModbusMessage Parse(ReadOnlySpan<byte> buffer, ModbusMessageType type)
            => new ExceptionMessage(buffer);
    }
}