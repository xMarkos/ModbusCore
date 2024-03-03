using System;
using ModbusCore.Messages;

namespace ModbusCore.Parsers
{
    public class ReadDeviceIdentificationMessageParser : IMessageParser
    {
        public bool CanHandle(ReadOnlySpan<byte> buffer, ModbusMessageType type)
        {
            if ((ModbusFunctionCode)buffer[1] != ModbusFunctionCode.EncapsulatedInterfaceTransport)
                return false;

            // Getting an extra byte works thanks to the fact that we are reading 4 bytes into the buffer
            if (buffer.Length < 3)
                throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer must provide at least 3 bytes.");

            if (buffer[2] != ReadDeviceIdentificationRequestMessage.ReadDeviceIdentificationMeiType)
                return false;

            return type is ModbusMessageType.Request or ModbusMessageType.Response;
        }

        public bool TryGetFrameLength(ReadOnlySpan<byte> buffer, ModbusMessageType type, out int length)
        {
            length = 3;

            if (buffer.Length < length)
                return false;

            if (buffer[2] != ReadDeviceIdentificationRequestMessage.ReadDeviceIdentificationMeiType)
                throw new NotSupportedException();

            length = 8;

            if (buffer.Length < length)
                return false;

            int count = buffer[7];
            for (int i = 0; i < count; i++)
            {
                length += 2;
                if (buffer.Length < length)
                    return false;

                length += buffer[length - 1];
                if (buffer.Length < length)
                    return false;
            }

            return true;
        }

        public IModbusMessage Parse(ReadOnlySpan<byte> buffer, ModbusMessageType type)
        {
            int length = this.ValidateParse(buffer, type);

            if (type == ModbusMessageType.Request)
            {
                /*
                 1: address of slave
                 1: function (0x2B)
                 1: MEI type (0x0E)
                 1: Read device Id code (1-4)
                 1: Object Id
                 */

                return new ReadDeviceIdentificationRequestMessage(buffer[..length]);
            }
            else
            {
                /*
                 1: address of slave
                 1: function
                 1: MEI type
                 1: Read device Id code (1-4)
                 1: Conformity level (0x1, 0x2, 0x3, 0x81, 0x82, 0x83)
                 1: More follows (0, 0xFF)
                 1: Next object Id
                 1: Count of objects
                 *: 1: Object Id
                    1: Object length
                    n: data (of length from previous field)
                 */

                return new ReadDeviceIdentificationResponseMessage(buffer[..length]);
            }
        }
    }
}
