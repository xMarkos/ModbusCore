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

        public bool IsTransactionActive(Transaction transaction)
            => _items.TryGetValue(transaction, out _);

        public void AddTransaction(Transaction transaction)
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            using ICacheEntry cacheEntry = _items.CreateEntry(transaction);
            cacheEntry.AbsoluteExpirationRelativeToNow = Timeout;

            // Note: The value MUST be set, otherwise the cache entry wouldn't get commited to the cache!
            cacheEntry.Value = null;
        }

        public void RemoveTransaction(Transaction transaction)
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            _items.Remove(transaction);
        }
    }

    public class ExpiringMessagingContext<TContext> : ExpiringMessagingContext
    {
        private readonly PostEvictionDelegate? _evictionCallback;

        public ExpiringMessagingContext(bool disposeValueOnExpiration = true)
        {
            if (typeof(IDisposable).IsAssignableFrom(typeof(TContext)) && disposeValueOnExpiration)
            {
                _evictionCallback = static (key, value, reason, state) =>
                {
                    if (reason is EvictionReason.Capacity or EvictionReason.Expired or EvictionReason.TokenExpired && value is IDisposable d)
                        d.Dispose();
                };
            }
        }

        public bool TryGetActiveTransaction(Transaction transaction, out TContext? context)
        {
            bool success = _items.TryGetValue(transaction, out object value);
            context = value is TContext t ? t : default;
            return success;
        }

        public void AddTransaction(Transaction transaction, TContext context)
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            using ICacheEntry cacheEntry =
                _items.CreateEntry(transaction)
                    .SetValue(context)
                    .SetAbsoluteExpiration(Timeout)
                    .RegisterPostEvictionCallback(_evictionCallback);
        }
    }
}
