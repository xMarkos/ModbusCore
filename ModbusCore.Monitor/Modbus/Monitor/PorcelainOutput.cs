namespace ModbusCore.Monitor
{
    internal class PorcelainOutput
    {
        public bool IsRequest { get; set; }
        public IModbusMessage? Message { get; set; }

        public PorcelainOutput() { }

        public PorcelainOutput(bool isRequest, IModbusMessage message)
        {
            IsRequest = isRequest;
            Message = message;
        }
    }
}
