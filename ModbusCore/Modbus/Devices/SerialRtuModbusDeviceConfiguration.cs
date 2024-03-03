using System.IO.Ports;

namespace ModbusCore.Devices;

#nullable disable
public class SerialRtuModbusDeviceConfiguration
{
    public string PortName { get; set; }
    public int BaudRate { get; set; }
    public Parity Parity { get; set; }
    public StopBits? StopBits { get; set; }
}
