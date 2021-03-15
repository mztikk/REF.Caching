namespace REF.Caching
{
    public interface IRemoveableCache<TValue, TKey> : ICache<TValue, TKey> where TKey : notnull
    {
        void Remove(TKey key);
    }
}
