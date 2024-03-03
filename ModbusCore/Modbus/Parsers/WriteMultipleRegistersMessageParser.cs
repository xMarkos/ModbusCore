using System;
using ModbusCore.Messages;

namespace ModbusCore.Parsers
{
    public class WriteMultipleRegistersMessageParser : IMessageParser
    {
        public bool CanHandle(ReadOnlySpan<byte> buffer, ModbusMessageType type)
            => CanHandle((ModbusFunctionCode)buffer[1], type);

        public bool CanHandle(ModbusFunctionCode function, ModbusMessageType type)
            => type is ModbusMessageType.Request or ModbusMessageType.Response && function is ModbusFunctionCode.WriteMultipleHoldingRegisters;

        public bool TryGetFrameLength(ReadOnlySpan<byte> buffer, ModbusMessageType type, out int length)
        {
            switch (type)
            {
                case ModbusMessageType.Request:
                    if (buffer.Length < 7)
                    {
                        length = 7;
                        return false;
                    }

                    length = buffer[6] + 7;
                    return true;
                case ModbusMessageType.Response:
                    length = 6;
                    return true;
                default:
                    throw new NotSupportedException();
            }
        }

        public IModbusMessage Parse(ReadOnlySpan<byte> buffer, ModbusMessageType type)
        {
            int length = this.ValidateParse(buffer, type);

            if (type == ModbusMessageType.Request)
            {
                /*
                 1: address of slave
                 1: function
                 2: start register (register num - 1)
                 2: count of registers
                 1: length of data (count of registers * 2)
                 n: data
                 */

                return new WriteMultipleRegistersRequestMessage(buffer[..length]);
            }
            else
            {
                /*
                 1: address of slave (echo)
                 1: function (echo)
                 2: start register (register num - 1)
                 2: count of registers
                 */

                return new WriteMultipleRegistersResponseMessage(buffer[..length]);
            }
        }
    }
}
