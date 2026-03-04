using Sackrany.Actor.Unit;
using Sackrany.Utils.Hash;
using Sackrany.Utils.Pool.Abstracts;

using UnityEngine;

namespace Sackrany.Utils.Pool.Extensions
{
    public static class PoolExtensions
    {
        public static GameObject POOL(this GameObject gameObject, Transform parent = null)
        {
            var pool = PoolManager.GetOrCreatePool(gameObject, gameObject.name.XXHash());
            return pool.Get(parent).gameObject;
        }
        public static Unit POOL(this Unit unit, Transform parent = null)
        {
            var pool = PoolManager.GetOrCreatePool(unit.gameObject, unit.Archetype.Hash);
            return pool.Get(parent).gameObject.GetComponent<Unit>();
        }
        
        public static void RELEASE(this GameObject gameObject)
        {
            var pool = PoolManager.GetOrCreatePool(gameObject, gameObject.name.XXHash());
            if (gameObject.TryGetComponent(out IPoolable poolable))
                pool.Release(poolable);
            else
                GameObject.Destroy(gameObject);
        }
        public static void RELEASE(this IPoolable poolable)
        {
            var pool = PoolManager.GetOrCreatePool(poolable.gameObject, poolable.gameObject.name.XXHash());
            pool.Release(poolable);
        }
        public static void RELEASE(this Unit unit)
        {
            var pool = PoolManager.GetOrCreatePool(unit.gameObject, unit.Archetype.Hash);
            pool.Release(unit);
        }
    }
}