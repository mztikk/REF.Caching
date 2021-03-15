using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace REF.Caching
{
    public class DbCache<TValue, TKey, TDbContext> : ICache<TValue, TKey>
    where TValue : class
    where TKey : notnull
    where TDbContext : DbContext
    {
        private static readonly SemaphoreSlim s_lock = new(1, 1);

        private readonly ILogger<DbCache<TValue, TKey, TDbContext>> _logger;
        private readonly TDbContext _dbContext;

        public DbCache(ILogger<DbCache<TValue, TKey, TDbContext>> logger, TDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public virtual TValue? Get(TKey key)
        {
            _logger.LogDebug("Accessing '{key}' directly in {Type}", key, GetType());
            return Find(key);
        }

        public virtual bool TryGet(TKey key, [NotNullWhen(true)] out TValue? value)
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

        public virtual void Add(TKey key, TValue value)
        {
            s_lock.Wait();

            try
            {
                if (Find(key) is null)
                {
                    value = InternalAdd(key, value);
                    _logger.LogDebug("'{key}' added to {Type} with value '{value}'", key, GetType(), value);
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

        protected virtual TValue? Find(TKey key) => _dbContext.Find<TValue>(key);

        protected virtual TValue InternalAdd(TKey key, TValue value)
        {
            _dbContext.Add(value);

            return value;
        }
    }
}
