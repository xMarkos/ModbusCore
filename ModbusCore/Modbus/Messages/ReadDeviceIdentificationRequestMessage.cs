using System;

namespace ModbusCore.Messages
{
    public record ReadDeviceIdentificationRequestMessage : MessageBase
    {
        public const byte ReadDeviceIdentificationMeiType = 0x2e;

        public byte MeiType { get; } = ReadDeviceIdentificationMeiType;
        public DeviceIdCodeType DeviceIdCode { get; init; }
        public byte ObjectId { get; init; }

        public ReadDeviceIdentificationRequestMessage()
        {
        }

        public ReadDeviceIdentificationRequestMessage(ReadOnlySpan<byte> buffer)
            : base(buffer)
        {
            ValidateBufferLength(buffer, 5);

            byte meiType = buffer[2];
            DeviceIdCode = (DeviceIdCodeType)buffer[3];
            ObjectId = buffer[4];

            if (meiType != ReadDeviceIdentificationMeiType)
                throw new FormatException($"{nameof(MeiType)} (idx:2) must be {ReadDeviceIdentificationMeiType:x}.");
        }

        public override bool TryWriteTo(Span<byte> buffer, out int length)
        {
            base.TryWriteTo(buffer, out length);
            length += 3;

            if (buffer.Length < length)
                return false;

            ModbusUtility.Write(buffer[2..], MeiType);
            ModbusUtility.Write(buffer[3..], (byte)DeviceIdCode);
            ModbusUtility.Write(buffer[4..], ObjectId);
            return true;
        }

        public enum DeviceIdCodeType : byte
        {
            Basic = 1,
            Regular = 2,
            Extended = 3,
            Individual = 4,
        }
    }
}
