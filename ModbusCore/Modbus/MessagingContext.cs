using System;
using System.Collections.Concurrent;

namespace ModbusCore;

public class MessagingContext : IMessagingContext
{
    private readonly ConcurrentDictionary<Transaction, bool> _transactions = new();

    public bool IsTransactionActive(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        return _transactions.ContainsKey(transaction);
    }

    public void AddTransaction(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        _transactions.TryAdd(transaction, true);
    }

    public void RemoveTransaction(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        _transactions.TryRemove(transaction, out _);
    }
}
