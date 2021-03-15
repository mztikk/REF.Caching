using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace REF.Caching
{
    public class MemoryBackedDbCache<TValue, TKey, TDbContext> : IRemoveableCache<TValue, TKey>
    where TValue : class
    where TKey : notnull
    where TDbContext : DbContext
    {
        private static readonly SemaphoreSlim s_lock = new(1, 1);

        private readonly ILogger<MemoryBackedDbCache<TValue, TKey, TDbContext>> _logger;
        protected readonly TDbContext _dbContext;
        protected readonly MemoryCache<TValue, TKey> _memoryCache;

        public MemoryBackedDbCache(
            ILogger<MemoryBackedDbCache<TValue, TKey, TDbContext>> logger,
            TDbContext dbContext,
            MemoryCache<TValue, TKey> memoryCache)
        {
            _logger = logger;
            _dbContext = dbContext;
            _memoryCache = memoryCache;
        }

        public TValue? Get(TKey key)
        {
            _logger.LogDebug("Accessing '{key}' directly in {Type}", key, GetType());
            return Find(key);
        }

        public bool TryGet(TKey key, [NotNullWhen(true)] out TValue? value)
        {
            value = Find(key);
            if (value is not null)
            {
                _logger.LogDebug("'{key}' found in {Type} with value '{value}'", key, GetType(), value);
                return true;
            }
            else
            {
                _logger.LogDebug("'{key}' not found in {Type}", key, GetType());
                return false;
            }
        }

        public void Add(TKey key, TValue value)
        {
            s_lock.Wait();

            try
            {
                if (Find(key) is null)
                {
                    value = InternalAdd(key, value);
                    _logger.LogDebug("'{key}' added to {Type} with value '{value}'", key, GetType(), value.ToString());
                    _dbContext.SaveChanges();
                }
                else
                {
                    _logger.LogDebug("'{key}' already in {Type}", key, GetType());
                }
            }
            finally
            {
                s_lock.Release();
            }
        }

        public void Remove(TKey key)
        {
            _memoryCache.Remove(key);

            TValue? val = DirectFind(key);
            if (val is not null)
            {
                _dbContext.Remove(val);
                _dbContext.SaveChanges();
            }
        }

        protected virtual TValue? Find(TKey key)
        {
            if (_memoryCache.TryGet(key, out TValue? value))
            {
                return value;
            }
            if ((value = DirectFind(key)) is not null)
            {
                _memoryCache.Add(key, value);
                return value;
            }

            return null;
        }

        protected virtual TValue? DirectFind(TKey key) => _dbContext.Find<TValue>(key);

        protected virtual TValue InternalAdd(TKey key, TValue value)
        {
            _dbContext.Add(value);
            _memoryCache.Add(key, value);

            return value;
        }
    }
}
