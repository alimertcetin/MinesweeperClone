using System;
using System.Collections.Generic;

namespace XIV.PoolSystem
{
    public static class XIVPoolSystem
    {
        static Dictionary<Type, IPool> pools;

        [UnityEngine.RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            pools = new Dictionary<Type, IPool>();
        }
        
        public static T AddPool<T>(T pool) where T : IPool
        {
            pools.Add(pool.StoredType, pool);
            return pool;
        }

        public static bool RemovePool<T>(T pool) where T : IPool<IPoolable>
        {
            return pools.Remove(pool.StoredType);
        }

        public static T GetItem<T>() where T : IPoolable
        {
            if (HasPool<T>() == false) throw new NullReferenceException("Pool is null. You should add a pool for " + typeof(T) + " to get items from it");
            return GetPool<T>().GetItem();
        }

        static IPool<T> GetPool<T>() where T : IPoolable
        {
            if (pools.TryGetValue(typeof(T), out var value))
            {
                return (IPool<T>)value;
            }

            return default;
        }

        public static bool HasPool<T>() where T : IPoolable
        {
            return pools.ContainsKey(typeof(T));
        }
    }
}