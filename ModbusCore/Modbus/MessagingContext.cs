using System.Collections.Concurrent;

namespace ModbusCore
{
    public class MessagingContext
    {
        private readonly ConcurrentDictionary<Transaction, bool> _transactions = new();

        public bool IsRequestActive(Transaction transaction)
            => _transactions.ContainsKey(transaction);

        public void AddTransaction(Transaction transaction)
            => _transactions.TryAdd(transaction, true);

        public void RemoveTransaction(Transaction transaction)
            => _transactions.TryRemove(transaction, out _);
    }
}
