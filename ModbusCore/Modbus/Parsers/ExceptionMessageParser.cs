﻿using System;
using ModbusCore.Messages;

namespace ModbusCore.Parsers;

public class ExceptionMessageParser : IMessageParser
{
    public bool CanHandle(ReadOnlySpan<byte> buffer, ModbusMessageType type)
        => (buffer[1] & 0b1000_0000) != 0;

    public bool TryGetFrameLength(ReadOnlySpan<byte> buffer, ModbusMessageType type, out int length)
    {
        length = 3;
        return true;
    }

    public IModbusMessage Parse(ReadOnlySpan<byte> buffer, ModbusMessageType type)
    {
        int length = this.ValidateParse(buffer, type);
        return new ExceptionMessage(buffer[..length]);
    }
}
