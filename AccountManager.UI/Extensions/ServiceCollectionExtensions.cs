using AccountManager.UI.Builders;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.UI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IGenericFactoryBuilder<TKey, TInterface> AddFactory<TKey, TInterface>(this ServiceCollection services) where TKey : notnull, new()
        {
            return new GenericFactoryBuilder<TKey, TInterface>(services);
        }
    }
}
