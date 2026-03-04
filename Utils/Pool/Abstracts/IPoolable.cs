using UnityEngine;

namespace Sackrany.Utils.Pool.Abstracts
{
    public interface IPoolable
    {
        public string name { get; set; }
        public GameObject gameObject { get; }
        public void OnPooled();
        public void OnReleased();
    }
}