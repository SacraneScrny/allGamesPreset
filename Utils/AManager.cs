using System;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Sackrany.Utils
{
    public abstract class AManager<T> : MonoBehaviour where T : AManager<T>
    {
        private protected static T _instance;
        private bool _initialized;
        
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindAnyObjectByType<T>(FindObjectsInactive.Exclude);
                    
                    if (_instance == null)
                    {
                        var go = new GameObject(typeof(T).Name);
                        _instance = go.AddComponent<T>();
                    }

                    Instance.Initialize();
                }

                return _instance;
            }
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            gameObject.name = typeof(T).Name;
        }
        #endif

        private void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            OnInitialize();
        }
        private protected virtual void OnInitialize() { }
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = (T)this;
            Instance.Initialize();
            OnManagerAwake();
        }
        private protected virtual void OnManagerAwake() { }

        private void OnDestroy()
        {
            OnManagerDestroy();
            if (_instance == this)
                _instance = null;
        }
        private protected virtual void OnManagerDestroy() { }
    }
}