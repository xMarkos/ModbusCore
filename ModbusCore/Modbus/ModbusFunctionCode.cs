namespace ModbusCore;

public enum ModbusFunctionCode : byte
{
    ReadCoils = 1,
    ReadDiscreteInputs = 2,
    ReadHoldingRegisters = 3,
    ReadInputRegisters = 4,
    WriteSingleCoil = 5,
    WriteSingleHoldingRegister = 6,
    ReadExceptionStatus = 7,
    Diagnostic = 8, // (+ subcodes 00-04,10-18,20) [page 22]
    GetComEventCounter = 11,
    GetComEventLog = 12,
    WriteMultipleCoils = 15,
    WriteMultipleHoldingRegisters = 16,
    ReportServerId = 17,
    ReadFileRecord = 20,
    WriteFileRecord = 21,
    MaskWriteRegister = 22,
    ReadWriteMultipleRegisters = 23,
    ReadFifo = 24,
    EncapsulatedInterfaceTransport = 43,    // sub 13: encapsulated interface transport; sub 14: read device identification [page 50]*/
}
