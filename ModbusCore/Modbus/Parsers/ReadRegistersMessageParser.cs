using System;
using ModbusCore.Messages;

namespace ModbusCore.Parsers
{
    public class ReadRegistersMessageParser : IMessageParser
    {
        public bool CanHandle(ModbusFunctionCode function, ModbusMessageType type)
        {
            return type switch
            {
                ModbusMessageType.Request => function
                    is ModbusFunctionCode.ReadHoldingRegisters
                    or ModbusFunctionCode.ReadInputRegisters
                    or ModbusFunctionCode.ReadCoils
                    or ModbusFunctionCode.ReadDiscreteInputs,
                ModbusMessageType.Response => function
                    is ModbusFunctionCode.ReadHoldingRegisters
                    or ModbusFunctionCode.ReadInputRegisters
                    or ModbusFunctionCode.ReadWriteMultipleRegisters,
                _ => false,
            };
        }

        public bool TryGetFrameLength(ReadOnlySpan<byte> buffer, ModbusMessageType type, out int length)
        {
            switch (type)
            {
                case ModbusMessageType.Request:
                    length = 6;
                    return true;
                case ModbusMessageType.Response:
                    if (buffer.Length < 3)
                    {
                        length = 3;
                        return false;
                    }

                    length = 3 + buffer[2];
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
                 */

                return new ReadRegistersRequestMessage(buffer[..length]);
            }
            else
            {
                /*
                 1: address of slave (echo)
                 1: function (echo)
                 1: length of data (count of registers * 2)
                 n: data
                 */

                return new ReadRegistersResponseMessage(buffer[..length]);
            }
        }
    }
}
