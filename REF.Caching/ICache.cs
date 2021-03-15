using System.Diagnostics.CodeAnalysis;

namespace REF.Caching
{
    public interface ICache<TValue, TKey> where TKey : notnull
    {
        void Add(TKey key, TValue value);
        TValue? Get(TKey key);
        bool TryGet(TKey key, [NotNullWhen(true)] out TValue? etfInfo);
    }
}
