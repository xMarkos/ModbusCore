namespace ModbusCore.Messages
{
    public static class ModbusMessageBuilderExtensions
    {
        private static ReadRegistersRequestMessage ReadRequest(ModbusFunctionCode function, byte address, ushort register, ushort count)
        {
            return new ReadRegistersRequestMessage
            {
                Address = address,
                Function = function,
                Register = register,
                Count = count,
            };
        }

        public static ReadRegistersRequestMessage ReadCoilsRequest(this IModbusMessageBuilder _, byte address, ushort register, ushort count)
            => ReadRequest(ModbusFunctionCode.ReadCoils, address, register, count);

        public static ReadRegistersRequestMessage ReadDiscreteInputsRequest(this IModbusMessageBuilder _, byte address, ushort register, ushort count)
            => ReadRequest(ModbusFunctionCode.ReadDiscreteInputs, address, register, count);

        public static ReadRegistersRequestMessage ReadHoldingRegistersRequest(this IModbusMessageBuilder _, byte address, ushort register, ushort count)
            => ReadRequest(ModbusFunctionCode.ReadHoldingRegisters, address, register, count);

        public static ReadRegistersRequestMessage ReadInputRegistersRequest(this IModbusMessageBuilder _, byte address, ushort register, ushort count)
            => ReadRequest(ModbusFunctionCode.ReadInputRegisters, address, register, count);

        public static WriteSingleValueMessage WriteSingleCoilRequest(this IModbusMessageBuilder _, byte address, ushort register, bool value)
            => _.WriteSingleHoldingRegisterRequest(address, register, (ushort)(value ? 0xFF00 : 0));

        public static WriteSingleValueMessage WriteSingleHoldingRegisterRequest(this IModbusMessageBuilder _, byte address, ushort register, ushort value)
        {
            return new WriteSingleValueMessage
            {
                Address = address,
                Function = ModbusFunctionCode.WriteSingleHoldingRegister,
                Register = register,
                Value = value,
            };
        }

        public static ReadExceptionStatusRequestMessage ReadExceptionStatusRequest(this IModbusMessageBuilder _, byte address)
        {
            return new ReadExceptionStatusRequestMessage
            {
                Address = address,
                Function = ModbusFunctionCode.ReadExceptionStatus,
            };
        }

        public static RawModbusMessage RawMessage(this IModbusMessageBuilder _, byte[] frame, ModbusMessageType type)
            => new RawModbusMessage(frame, type);

        public static ReadDeviceIdentificationRequestMessage ReadDeviceIdentificationRequest(
            this IModbusMessageBuilder _,
            byte address,
            ReadDeviceIdentificationRequestMessage.DeviceIdCodeType deviceIdCode,
            byte objectId)
        {
            return new()
            {
                Address = address,
                Function = ModbusFunctionCode.EncapsulatedInterfaceTransport,
                DeviceIdCode = deviceIdCode,
                ObjectId = objectId,
            };
        }

        public static ReadDeviceIdentificationResponseMessage ReadDeviceIdentificationResponse(
            this IModbusMessageBuilder _,
            byte address,
            ReadDeviceIdentificationRequestMessage.DeviceIdCodeType deviceIdCode,
            ReadDeviceIdentificationResponseMessage.DeviceConformityLevel conformityLevel,
            bool moreFollows,
            byte nextObjectId,
            ReadDeviceIdentificationResponseMessage.ObjectRecord[] objects)
        {
            return new()
            {
                Address = address,
                Function = ModbusFunctionCode.EncapsulatedInterfaceTransport,
                DeviceIdCode = deviceIdCode,
                ConformityLevel = conformityLevel,
                MoreFollows = moreFollows,
                NextObjectId = nextObjectId,
                Objects = objects,
            };
        }
    }
}
