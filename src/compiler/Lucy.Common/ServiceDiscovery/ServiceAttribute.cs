using System;
using Microsoft.Extensions.DependencyInjection;

namespace Lucy.Common.ServiceDiscovery
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : Attribute
    {
        public ServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Singleton, Type? serviceType = null)
        {
            Lifetime = lifetime;
            ServiceType = serviceType;
        }

        public ServiceLifetime Lifetime { get; }
        public Type? ServiceType { get; }
    }
}
