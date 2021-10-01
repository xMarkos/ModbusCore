using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ModbusCore
{
    public class ExpiringMessagingContext : IMessagingContext
    {
        private readonly static IOptions<MemoryCacheOptions> _options =
            new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMilliseconds(100),
            });

        protected readonly MemoryCache _items;

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(1);

        public ExpiringMessagingContext()
        {
            _items = new(_options);
        }

        public bool IsRequestActive(Transaction transaction)
            => _items.TryGetValue(transaction, out _);

        public void AddTransaction(Transaction transaction)
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            using ICacheEntry cacheEntry = _items.CreateEntry(transaction);
            cacheEntry.AbsoluteExpirationRelativeToNow = Timeout;
        }

        public void RemoveTransaction(Transaction transaction)
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            _items.Remove(transaction);
        }
    }

    public class ExpiringMessagingContext<T> : ExpiringMessagingContext
    {
        public bool TryGetActiveRequest(Transaction transaction, out T? request)
        {
            bool success = _items.TryGetValue(transaction, out object value);
            request = value is T t ? t : default;
            return success;
        }

        public void AddTransaction(Transaction transaction, T request)
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            _items.Set(transaction, request, Timeout);
        }
    }
}
