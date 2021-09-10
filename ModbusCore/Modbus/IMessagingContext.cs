namespace ModbusCore
{
    public interface IMessagingContext
    {
        bool IsRequestActive(Transaction transaction);
    }
}
