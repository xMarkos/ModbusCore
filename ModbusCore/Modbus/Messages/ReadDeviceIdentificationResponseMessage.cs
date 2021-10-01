using System;
using System.Linq;
using static ModbusCore.Messages.ReadDeviceIdentificationRequestMessage;

namespace ModbusCore.Messages
{
    /// <summary>
    /// Modbus message representing response for function codes:
    /// <list type="bullet">
    /// <item><see cref="ModbusFunctionCode.EncapsulatedInterfaceTransport"/> MEI code 0x2E</item>
    /// </list>
    /// </summary>
    public record ReadDeviceIdentificationResponseMessage : MessageBase
    {
        public byte MeiType { get; } = ReadDeviceIdentificationMeiType;
        public DeviceIdCodeType DeviceIdCode { get; init; }
        public DeviceConformityLevel ConformityLevel { get; init; }
        public bool MoreFollows { get; init; }
        public byte NextObjectId { get; init; }

        public byte ObjectCount => checked((byte)Objects.Length);

        private readonly ObjectRecord[] _objects = null!;
        public ObjectRecord[] Objects
        {
            get => _objects;
            init => _objects = value ?? Array.Empty<ObjectRecord>();
        }

        public ReadDeviceIdentificationResponseMessage() : base(ModbusMessageType.Response)
            => Objects = null!;

        public ReadDeviceIdentificationResponseMessage(ReadOnlySpan<byte> buffer)
            : base(buffer, ModbusMessageType.Response)
        {
            ValidateBufferLength(buffer, 8);

            byte meiType = buffer[2];
            DeviceIdCode = (DeviceIdCodeType)buffer[3];
            ConformityLevel = (DeviceConformityLevel)buffer[4];
            MoreFollows = buffer[5] != 0;
            NextObjectId = buffer[6];

            byte objectCount = buffer[7];

            if (meiType != ReadDeviceIdentificationMeiType)
                throw new FormatException($"{nameof(MeiType)} (idx:2) must be {ReadDeviceIdentificationMeiType:x}.");

            if (!Enum.IsDefined(DeviceIdCode))
                throw new FormatException($"{nameof(DeviceIdCode)} (idx:3) is invalid ({DeviceIdCode}).");

            if (!Enum.IsDefined(ConformityLevel))
                throw new FormatException($"{nameof(ConformityLevel)} (idx:4) is invalid ({ConformityLevel}).");

            Objects = new ObjectRecord[objectCount];

            int length = 8;

            for (int i = 0; i < objectCount; i++)
            {
                ValidateBufferLength(buffer, length + 2);

                byte id = buffer[length];
                byte objLength = buffer[length + 1];

                ValidateBufferLength(buffer, length + 2 + objLength);

                Objects[i] = new(id, buffer.Slice(length + 2, objLength).ToArray());

                length += 2 + objLength;
            }
        }

        public override bool TryWriteTo(Span<byte> buffer, out int length)
        {
            base.TryWriteTo(buffer, out length);
            length += 6 + Objects.Sum(x => 2 + x.Length);

            if (buffer.Length < length)
                return false;

            ModbusUtility.Write(buffer[2..], MeiType);
            ModbusUtility.Write(buffer[3..], (byte)DeviceIdCode);
            ModbusUtility.Write(buffer[4..], (byte)ConformityLevel);
            ModbusUtility.Write(buffer[5..], MoreFollows ? 0xFF : 0);
            ModbusUtility.Write(buffer[6..], NextObjectId);
            ModbusUtility.Write(buffer[7..], ObjectCount);

            int idx = 8;

            foreach (ObjectRecord record in Objects)
            {
                ModbusUtility.Write(buffer[idx++..], record.Id);
                ModbusUtility.Write(buffer[idx++..], record.Length);
                record.Value.AsSpan().CopyTo(buffer[idx++..]);
            }

            return true;
        }

        public enum DeviceConformityLevel : byte
        {
            Basic = 1,
            Regular = 2,
            Extended = 3,
            BasicIndividual = 0x81,
            RegularIndividual = 0x80,
            ExtendedIndividual = 0x83,
        }

        public record ObjectRecord(byte Id, byte[] Value)
        {
            public byte Length => checked((byte)_value.Length);

            private byte[] _value = Value ?? Array.Empty<byte>();
            public byte[] Value
            {
                get => _value;
                init => _value = value ?? Array.Empty<byte>();
            }
        }
    }
}
