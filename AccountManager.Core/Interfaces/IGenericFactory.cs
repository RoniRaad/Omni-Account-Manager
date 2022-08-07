namespace AccountManager.Core.Interfaces
{
    public interface IGenericFactory<TKey, TInterface> where TKey : notnull, new()
    {
        TInterface CreateImplementation(TKey key);
    }
}