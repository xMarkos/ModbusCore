using System.Collections.Generic;

namespace ModbusCore.Parsers
{
    public static class ParserCollection
    {
        public static IReadOnlyCollection<IMessageParser> Default { get; } =
            new List<IMessageParser>
            {
                new ExceptionMessageParser(),
                new ReadCoilsResponseMessageParser(),
                new ReadRegistersMessageParser(),
                new ReadExceptionStatusMessageParser(),
                new WriteSingleValueMessageParser(),
                new WriteMultipleRegistersMessageParser(),
                new ReadWriteMultipleRegistersRequestMessageParser(),
            }.AsReadOnly();
    }
}
