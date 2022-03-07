using AccountManager.Core.Interfaces;

namespace AccountManager.Core.Factories
{
    public class GenericFactory<TKey, TInterface> : IFactory<TKey, TInterface> where TKey : notnull, new()
    {
        private Dictionary<TKey, Type> _implementations;
        private IServiceProvider _serviceProvider;
        public GenericFactory(Dictionary<TKey, Type> implementations, IServiceProvider serviceProvider)
        {
            _implementations = implementations;
            _serviceProvider = serviceProvider;
        }
        public TInterface CreateImplementation(TKey key)
        {
            var implementationType = _implementations[key];
            var implementation = _serviceProvider.GetService(implementationType);
            return (TInterface)implementation;
        }
    }
}
