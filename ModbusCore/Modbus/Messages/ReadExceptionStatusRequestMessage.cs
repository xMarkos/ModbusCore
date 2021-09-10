using System;

namespace ModbusCore.Messages
{
    public record ReadExceptionStatusRequestMessage : MessageBase
    {
        public ReadExceptionStatusRequestMessage() { }
        public ReadExceptionStatusRequestMessage(ReadOnlySpan<byte> buffer) : base(buffer) { }
    }
}
