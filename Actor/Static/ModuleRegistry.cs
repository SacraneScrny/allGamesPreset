using System;

using Sackrany.Actor.Modules.Modules;
using Sackrany.Utils.CacheRegistry;

namespace Sackrany.Actor.Static
{
    public static class ModuleRegistry
    {
        public static int Count                          => TypeRegistry<Module>.Count;
        public static int GetId<T>() where T : Module   => TypeRegistry<Module>.Id<T>.Value;
        public static int GetId(Type type)               => TypeRegistry<Module>.GetOrRegister(type);
        public static Type GetTypeById(int id)           => TypeRegistry<Module>.GetTypeById(id);
        internal static int LookupId(Type type)          => TypeRegistry<Module>.GetOrRegister(type);
    }
}