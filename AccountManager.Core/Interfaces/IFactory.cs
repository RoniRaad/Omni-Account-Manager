namespace AccountManager.Core.Interfaces
{
    public interface IFactory<in TKey, out TInterface> where TKey : notnull, new()
    {
        TInterface CreateImplementation(TKey key);
    }
}