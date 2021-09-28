﻿using System;

namespace ModbusCore.Messages
{
    public record ExceptionMessage : MessageBase
    {
        public ModbusFunctionCode OriginalFunction { get; }
        public byte ExceptionCode { get; init; }

        public ExceptionMessage() { }

        public ExceptionMessage(ReadOnlySpan<byte> buffer)
            : base(buffer)
        {
            ValidateBufferLength(buffer, 3);

            OriginalFunction = ModbusUtility.GetFunctionCodeFromException(Function);
            ExceptionCode = buffer[2];
        }

        public override bool TryWriteTo(Span<byte> buffer, out int length)
        {
            base.TryWriteTo(buffer, out length);
            length += 1;

            if (buffer.Length < length)
                return false;

            buffer[2] = ExceptionCode;
            return true;
        }
    }
}
