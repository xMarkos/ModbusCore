namespace ModbusCore;

public enum ModbusExceptionCode : byte
{
    IllegalFunction = 1,
    IllegalDataAddress = 2,
    IllegalDataValue = 3,
    ServerDeviceFailure = 4,
    Asknowledge = 5,
    ServerDeviceBusy = 6,
    MemoryParityError = 8,
    GatewayPathUnavailable = 0xA,
    GatewayTargetDeviceFailedToRespons = 0xB,
}
