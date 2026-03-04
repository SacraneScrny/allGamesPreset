using System.Collections.Generic;

using Sackrany.Utils.Pool.Abstracts;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Sackrany.Utils.Pool
{
    public class Pool
    {
        private PoolItem _prefab;
        private Stack<IPoolable> _pool = new();
        private Transform _parent;
        
        public void Initialize(PoolItem prefab, int prewarm = 0)
        {
            _prefab = prefab;
            CreateParent();

            for (int i = 0; i < prewarm; i++)
                _pool.Push(Create());
        }
        private void CreateParent()
        {
            var allPools = GameObject.Find("Pools");
            if (allPools == null)
                allPools = new GameObject("Pools");
            _parent = new GameObject($"{_prefab.name}_Pool").transform;
            _parent.SetParent(allPools.transform);
        }
        
        public IPoolable Get(Transform parent = null)
        {
            IPoolable obj = _pool.Count > 0 ? _pool.Pop() : Create();
            
            obj.gameObject.transform.SetParent(parent);
            obj.OnPooled();
            return obj;
        }

        public void Release(IPoolable obj)
        {
            obj.OnReleased();
            _pool.Push(obj);
            obj.gameObject.transform.SetParent(_parent);
        }
        private IPoolable Create()
        {
            var go = Object.Instantiate(_prefab.gameObject);
            go.name = _prefab.name;
            var poolable = go.GetComponent<IPoolable>();

            go.SetActive(false);
            return poolable;
        }
    }

    public class PoolItem
    {
        private readonly GameObject _poolable;
        public PoolItem(GameObject poolable)
        {
            _poolable = poolable;
        }
        
        public string name => _poolable.name;
        public GameObject gameObject => _poolable;

        public void SetActive(bool active)
            => gameObject.SetActive(active);

        public U GetComponent<U>()
            => gameObject.GetComponent<U>();
    }
}