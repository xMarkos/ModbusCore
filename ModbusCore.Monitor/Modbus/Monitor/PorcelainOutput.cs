namespace ModbusCore.Monitor
{
    internal class PorcelainOutput : PorcelainOutputBase
    {
        public IModbusMessage? Message { get; set; }

        public PorcelainOutput(bool isRequest, IModbusMessage message)
            : base(isRequest)
        {
            IsRequest = isRequest;
            Message = message;
        }
    }
}
