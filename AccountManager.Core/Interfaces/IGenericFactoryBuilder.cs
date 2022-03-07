namespace AccountManager.Core.Interfaces
{
    public interface IGenericFactoryBuilder<TKey, TInterface> where TKey : notnull, new()
    {
        IGenericFactoryBuilder<TKey, TInterface> AddImplementation<TImplementation>(TKey key) where TImplementation : class, TInterface;
        void Build();
    }
}