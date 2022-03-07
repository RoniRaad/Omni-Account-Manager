namespace AccountManager.Core.Interfaces
{
    public interface IFactory<TKey, TInterface> where TKey : notnull, new()
    {
        TInterface CreateImplementation(TKey key);
    }
}