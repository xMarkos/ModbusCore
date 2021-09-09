using System;

namespace ModbusCore.Messages
{
    public record ReadRegistersRequestMessage : MessageBase
    {
        public ushort Register => ModbusUtility.ReadUInt16(Frame.AsSpan(2, 2));
        public ushort Count => ModbusUtility.ReadUInt16(Frame.AsSpan(4, 2));

        public ReadRegistersRequestMessage(byte[] frame) : base(frame) { }
    }
}
