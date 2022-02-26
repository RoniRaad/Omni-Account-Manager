namespace AccountManager.UI.Builders
{
    public interface IGenericFactoryBuilder<TKey, TInterface> where TKey : notnull, new()
    {
        IGenericFactoryBuilder<TKey, TInterface> AddImplementation<TImplementation>(TKey key) where TImplementation : TInterface, new();
        void Build();
    }
}