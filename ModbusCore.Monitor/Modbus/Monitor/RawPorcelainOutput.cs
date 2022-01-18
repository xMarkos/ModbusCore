using Newtonsoft.Json;

namespace ModbusCore.Monitor
{
    internal class RawPorcelainOutput : PorcelainOutputBase
    {
        [JsonProperty(PropertyName = "A")]
        public byte Address { get; }

        [JsonProperty(PropertyName = "F")]
        public byte Function { get; }

        public byte[] Frame { get; }

        public RawPorcelainOutput(bool isRequest, byte[] frame)
            : base(isRequest)
        {
            Frame = frame;
            Address = frame[0];
            Function = frame[1];
        }
    }
}
