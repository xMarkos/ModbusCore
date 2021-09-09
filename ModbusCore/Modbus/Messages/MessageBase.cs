using System;
using System.Text;

namespace ModbusCore.Messages
{
    public record MessageBase(byte[] Frame) : IModbusMessage
    {
        public byte Address => Frame[0];
        public byte Function => Frame[1];

        protected virtual bool PrintMembers(StringBuilder stringBuilder)
        {
            if (stringBuilder is null)
                throw new ArgumentNullException(nameof(stringBuilder));

            stringBuilder.Append($"{nameof(Address)} = {Address}, ");
            stringBuilder.Append($"{nameof(Function)} = {Function}");
            return true;
        }
    }
}
