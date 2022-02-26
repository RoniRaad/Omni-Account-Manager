using AccountManager.Core.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.UI.Builders
{
    public class GenericFactoryBuilder<TKey, TInterface> : IGenericFactoryBuilder<TKey, TInterface> where TKey : notnull, new()
    {
        private Dictionary<TKey, TInterface> _implementations { get; set; }
        private IServiceCollection _services;
        public GenericFactoryBuilder(IServiceCollection services)
        {
            _implementations = new Dictionary<TKey, TInterface>();
            _services = services;
        }
        public IGenericFactoryBuilder<TKey, TInterface> AddImplementation<TImplementation>(TKey key) where TImplementation : TInterface, new()
        {
            _implementations.Add(key, new TImplementation());
            return this;
        }

        public void Build()
        {
            _services.AddSingleton(services =>
            {
                return new GenericFactory<TKey, TInterface>(_implementations);
            });
        }
    }
}
