namespace ModbusCore
{
    public interface IModbusMessage
    {
        public byte[] Frame { get; }
        public byte Address { get; }
        public byte Function { get; }
    }
}
