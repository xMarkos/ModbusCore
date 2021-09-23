using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ModbusCore.Monitor
{
    public class ExpiringMessagingContext : MemoryCache, IMessagingContext
    {
        private readonly static IOptions<MemoryCacheOptions> _options =
            new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMilliseconds(100),
            });

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(1);

        public ExpiringMessagingContext(ILoggerFactory loggerFactory)
            : base(_options, loggerFactory)
        {
        }

        public bool IsRequestActive(Transaction transaction)
            => TryGetValue(transaction, out _);

        public void AddTransaction(Transaction transaction)
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            this.Set(transaction, true, Timeout);
        }

        public void RemoveTransaction(Transaction transaction)
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            Remove(transaction);
        }
    }
}
