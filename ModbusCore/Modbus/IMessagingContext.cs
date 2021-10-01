namespace ModbusCore
{
    public interface IMessagingContext
    {
        bool IsTransactionActive(Transaction transaction);
    }
}
