using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace REF.Caching
{
    public class MemoryCache<TValue, TKey> : IRemoveableCache<TValue, TKey>
    where TValue : class
    where TKey : notnull
    {
        private readonly ConcurrentDictionary<TKey, TValue> _cache = new();
        private readonly ILogger<TValue> _logger;

        public MemoryCache(ILogger<TValue> logger) => _logger = logger;

        public TValue? Get(TKey key)
        {
            if (TryGet(key, out TValue? value))
            {
                return value;
            }

            throw new Exception();
        }

        public bool TryGet(TKey key, [NotNullWhen(true)] out TValue? value)
        {
            if (_cache.TryGetValue(key, out value))
            {
                _logger.LogDebug("'{key}' found in {Type} with value '{value}'", key, GetType(), value);
                return true;
            }

            _logger.LogDebug("'{key}' not found in {Type}", key, GetType());
            return false;
        }

        public void Add(TKey key, TValue value)
        {
            if (_cache.TryAdd(key, value))
            {
                _logger.LogDebug("'{key}' added to {Type} with value '{value}'", key, GetType(), value);
            }
            else
            {
                _logger.LogDebug("Failed to add '{key}' to {Type} with value '{value}'", key, GetType(), value);
            }
        }

        public void Remove(TKey key) => _cache.TryRemove(key, out _);
    }
}
