namespace ModbusCore;

public static class ModbusFunctionCodeExtensions
{
    public static bool IsExceptionCode(this ModbusFunctionCode code)
        => ((int)code & 0b1000_0000) != 0;
}
