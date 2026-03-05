using System;
using System.Collections.Generic;
using System.Reflection;

using Sackrany.Actor.Modules;

namespace Sackrany.Actor.Static
{
    public static class TemplateReflectionCache
    {
        static readonly Dictionary<Type, HashKeyField[]> _cache = new();
        static readonly object _lock = new();

        public static HashKeyField[] GetHashKeys(Type templateType)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(templateType, out var cached))
                    return cached;

                var fields = templateType.GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

                var result = new List<HashKeyField>();
                foreach (var field in fields)
                    if (field.GetCustomAttribute<HashKeyAttribute>() is { } attr)
                        result.Add(new HashKeyField(field, attr));

                var arr = result.ToArray();
                _cache[templateType] = arr;
                return arr;
            }
        }

        public readonly struct HashKeyField
        {
            public readonly FieldInfo Field;
            public readonly HashKeyAttribute Attr;
            public HashKeyField(FieldInfo field, HashKeyAttribute attr)
            {
                Field = field;
                Attr  = attr;
            }
        }
    }
}