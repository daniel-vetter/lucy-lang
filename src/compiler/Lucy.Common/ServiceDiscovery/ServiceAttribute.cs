using System;

namespace Lucy.Common.ServiceDiscovery
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : Attribute
    {
        public ServiceAttribute(Lifetime lifetime = Lifetime.Singleton, Type? serviceType = null)
        {
            Lifetime = lifetime;
            ServiceType = serviceType;
        }

        public Lifetime Lifetime { get; }
        public Type? ServiceType { get; }
    }

    public enum Lifetime
    {
        Singleton,
        Scoped,
        Transient
    }
}
