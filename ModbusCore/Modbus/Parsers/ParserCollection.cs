using System.Collections.Generic;

namespace ModbusCore.Parsers;

public static class ParserCollection
{
    public static IReadOnlyCollection<IMessageParser> Default { get; } =
        new List<IMessageParser>
        {
            new ExceptionMessageParser(),
            new ReadCoilsResponseMessageParser(),
            new ReadDeviceIdentificationMessageParser(),
            new ReadExceptionStatusMessageParser(),
            new ReadRegistersMessageParser(),
            new ReadWriteMultipleRegistersRequestMessageParser(),
            new WriteMultipleRegistersMessageParser(),
            new WriteSingleValueMessageParser(),
        }.AsReadOnly();
}
