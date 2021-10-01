using System;
using System.Collections.Concurrent;

namespace ModbusCore
{
    public class MessagingContext : IMessagingContext
    {
        private readonly ConcurrentDictionary<Transaction, bool> _transactions = new();

        public bool IsTransactionActive(Transaction transaction)
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            return _transactions.ContainsKey(transaction);
        }

        public void AddTransaction(Transaction transaction)
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            _transactions.TryAdd(transaction, true);
        }

        public void RemoveTransaction(Transaction transaction)
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            _transactions.TryRemove(transaction, out _);
        }
    }
}
