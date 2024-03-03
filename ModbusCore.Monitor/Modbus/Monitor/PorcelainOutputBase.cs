using System;
using Newtonsoft.Json;

namespace ModbusCore.Monitor;

internal class PorcelainOutputBase(bool isRequest)
{
    [JsonProperty(Order = -9)]
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    [JsonProperty(Order = -8)]
    public bool IsRequest { get; set; } = isRequest;
}
