using System;
using System.Collections.Generic;

using Sackrany.Utils.CacheRegistry;

namespace Sackrany.Actor.Traits.Affinity
{
    public static class AffinityRegistry
    {
        static readonly Dictionary<Type, IAffinity> _instances = new();
        static readonly HashSet<Type> _setupCalled = new();
        static readonly object _lock = new();

        public static int Count                            => TypeRegistry<IAffinity>.Count;
        public static int GetId<T>() where T : IAffinity  => TypeRegistry<IAffinity>.Id<T>.Value;
        public static int GetId(Type type)                 => TypeRegistry<IAffinity>.GetId(type);
        public static Type GetTypeById(int id)             => TypeRegistry<IAffinity>.GetTypeById(id);
        internal static int LookupId(Type type)            => TypeRegistry<IAffinity>.GetOrRegister(type);

        public static IAffinity GetInstance<T>() where T : IAffinity => AffinityId<T>.Instance;
        internal static IAffinity LookupInstance(Type type)           => GetOrCreateInstance(type);

        static class AffinityId<T> where T : IAffinity
        {
            public static readonly int Value          = TypeRegistry<IAffinity>.Id<T>.Value;
            public static readonly IAffinity Instance = GetOrCreateInstance(typeof(T));
        }

        static IAffinity GetOrCreateInstance(Type type)
        {
            lock (_lock)
            {
                if (_instances.TryGetValue(type, out var inst))
                    return inst;

                inst = Activator.CreateInstance(type) as IAffinity;
                _instances[type] = inst;
            }

            bool needSetup;
            lock (_lock) needSetup = _setupCalled.Add(type);
            if (needSetup) _instances[type].Setup();

            return _instances[type];
        }
    }
}