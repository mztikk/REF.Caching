using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace REF.Caching
{
    public class TimedCache<TValue, TKey, TDbContext> : MemoryBackedDbCache<TValue, TKey, TDbContext>
    where TValue : Creation
    where TKey : notnull
    where TDbContext : DbContext
    {
        private readonly ILogger<TimedCache<TValue, TKey, TDbContext>> _logger;

        public virtual TimeSpan Expiry { get; set; } = TimeSpan.FromDays(7);

        public TimedCache(
            ILogger<TimedCache<TValue, TKey, TDbContext>> logger,
            TDbContext dbContext,
            MemoryCache<TValue, TKey> memoryCache) : base(logger, dbContext, memoryCache) => _logger = logger;

        protected override TValue? Find(TKey key)
        {
            TValue? val = base.Find(key);
            if (val is null)
            {
                return null;
            }

            if (IsExpired(val))
            {
                _logger.LogDebug("'{key}' expired in {Type}", key, GetType());

                Remove(key);
                return null;
            }

            return val;
        }

        protected virtual bool IsExpired(Creation creation) => DateTime.UtcNow - creation.CreatedAt > Expiry;
    }
}
