using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.ProjectManagement
{
    public class Cache
    {
        Dictionary<CacheKey, IVersionedEntity> _cache = new();

        public T? Get<T>(string identifier, int version) where T : IVersionedEntity
        {
            if (!_cache.TryGetValue(new CacheKey(identifier, typeof(T)), out var entity))
                return default(T);

            if (entity.Version != version)
                return default(T);

            return (T)entity;
        }

        public bool Get<T>(string identifier, int version, [NotNullWhen(true)] out T? result) where T : IVersionedEntity
        {
            result = Get<T>(identifier, version);
            if (result != null)
                return true;
            return false;
        }

        private record CacheKey(string Identifier, Type Type) { }

        internal void Set(string path, IVersionedEntity entity)
        {
            _cache[new CacheKey(path, entity.GetType())] = entity;
        }
    }

    public interface IVersionedEntity
    {
        int Version { get; }
    }
}
