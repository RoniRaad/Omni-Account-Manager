namespace AccountManager.Core.Interfaces
{
    public interface IGenericFactory<in TKey, out TInterface> where TKey : notnull, new()
    {
        TInterface CreateImplementation(TKey key);
    }
}