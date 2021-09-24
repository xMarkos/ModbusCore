﻿using System;
using System.Text;

namespace ModbusCore.Messages
{
    public record MessageBase : IModbusMessage
    {
        public byte Address { get; init; }
        public ModbusFunctionCode Function { get; init; }

        public MessageBase() { }

        public MessageBase(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < 2)
                throw new ArgumentException(null, nameof(buffer));

            Address = buffer[0];
            Function = (ModbusFunctionCode)buffer[1];
        }

        public virtual bool TryWriteTo(Span<byte> buffer, out int length)
        {
            length = 2;

            if (buffer.Length < length)
                return false;

            buffer[0] = Address;
            buffer[1] = (byte)Function;
            return true;
        }

        protected virtual bool PrintMembers(StringBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.AppendFormat("A ={0,3}, ", Address);
            builder.AppendFormat("F = {0}", Function);

            return true;
        }
    }
}
