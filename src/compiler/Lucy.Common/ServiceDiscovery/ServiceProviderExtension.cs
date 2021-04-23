using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace Lucy.Common.ServiceDiscovery
{
    public static class ServiceProviderExtension
    {
        public static IServiceCollection AddServicesFromCurrentAssembly(this IServiceCollection sc, bool printServices = false)
        {
            foreach (var type in Assembly.GetCallingAssembly().GetTypes())
            {
                var attribute = type.GetCustomAttribute<ServiceAttribute>();
                if (attribute != null)
                {
                    var serviceType = attribute.ServiceType;
                    if (serviceType == null)
                        serviceType = type.GetInterfaces().Length > 0 ? type.GetInterfaces()[0] : type;

                    if (printServices)
                        Console.WriteLine(serviceType + " -> " + type + ": " + attribute.Lifetime);
                    sc.Add(new ServiceDescriptor(serviceType, type, attribute.Lifetime));
                }
            }
            return sc;
        }
    }
}
