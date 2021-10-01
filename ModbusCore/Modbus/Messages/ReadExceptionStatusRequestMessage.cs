using System;

namespace ModbusCore.Messages
{
    public record ReadExceptionStatusRequestMessage : MessageBase
    {
        public ReadExceptionStatusRequestMessage() : base(ModbusMessageType.Request) { }
        public ReadExceptionStatusRequestMessage(ReadOnlySpan<byte> buffer) : base(buffer, ModbusMessageType.Request) { }
    }
}
