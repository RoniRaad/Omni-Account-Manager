using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Core.Factories
{
    public class GenericFactory<TKey, TInterface> : IFactory<TKey, TInterface> where TKey : notnull, new()
    {
        private Dictionary<TKey, TInterface> _implementations;
        public GenericFactory(Dictionary<TKey, TInterface> implementations)
        {
            _implementations = implementations;
        }
        public TInterface CreateImplementation(TKey key)
        {
            return _implementations[key];
        }
    }
}
