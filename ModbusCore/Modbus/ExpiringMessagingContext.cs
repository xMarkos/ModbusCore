using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ModbusCore
{
    public class ExpiringMessagingContext<T> : MemoryCache, IMessagingContext
    {
        private readonly static IOptions<MemoryCacheOptions> _options =
            new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMilliseconds(100),
            });

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(1);

        public ExpiringMessagingContext()
            : base(_options)
        {
        }

        public bool TryGetActiveRequest(Transaction transaction, [NotNullWhen(true)] out T? request)
        {
            if (TryGetValue(transaction, out object value) && value is T result)
            {
                request = result;
                return true;
            }

            request = default;
            return false;
        }

        public bool IsRequestActive(Transaction transaction)
            => TryGetValue(transaction, out _);

        public void AddTransaction(Transaction transaction, T request)
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            this.Set(transaction, request, Timeout);
        }

        public void RemoveTransaction(Transaction transaction)
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            Remove(transaction);
        }
    }
}
