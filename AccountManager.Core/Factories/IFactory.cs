namespace AccountManager.Core.Factories
{
    public interface IFactory<TKey, TInterface> where TKey : notnull, new()
    {
        TInterface CreateImplementation(TKey key);
    }
}