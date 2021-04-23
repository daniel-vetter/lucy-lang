using Lucy.Common.ServiceDiscovery;
using Microsoft.Extensions.DependencyInjection;

namespace Lucy.Feature.LanguageServer
{
    public static class LanguageServerFeatureModule
    {
        public static IServiceCollection AddLanguageServerFeatureModule(this IServiceCollection sc)
        {
            sc.AddServicesFromCurrentAssembly();
            return sc;
        }
    }
}
