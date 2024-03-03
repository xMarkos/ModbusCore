using Newtonsoft.Json;

namespace ModbusCore.Monitor;

internal class RawPorcelainOutput(bool isRequest, byte[] frame) : PorcelainOutputBase(isRequest)
{
    [JsonProperty(PropertyName = "A")]
    public byte Address { get; } = frame[0];

    [JsonProperty(PropertyName = "F")]
    public byte Function { get; } = frame[1];

    public byte[] Frame { get; } = frame;
}
