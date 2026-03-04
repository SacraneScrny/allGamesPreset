using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Sackrany.Actor.Modules;

using UnityEngine;

namespace Sackrany.Actor.Static
{
    public static class HashBuilder
    {
        public static uint Begin() => 2166136261u; // FNV-1a offset

        public static uint Add(uint hash, uint data) => (hash ^ data) * 16777619u;

        public static uint BuildFromTemplates(IEnumerable<object> templates)
        {
            uint hash = Begin();

            foreach (var t in templates
                         .OrderBy(t => t.GetType().FullName))
            {
                var type = t.GetType();
                hash = AddString(hash, type.FullName);

                foreach (var field in type.GetFields(
                             BindingFlags.Instance |
                             BindingFlags.Public |
                             BindingFlags.NonPublic))
                {
                    var attr = field.GetCustomAttribute<HashKeyAttribute>();
                    if (attr == null) continue;

                    var value = field.GetValue(t);
                    hash = AddValue(hash, value, attr);
                }
            }

            return hash;
        }
        private static uint AddValue(uint hash, object value, HashKeyAttribute attr)
        {
            if (value == null)
                return Add(hash, 0u);

            if (attr.IgnoreDefault && value.Equals(GetDefault(value.GetType())))
                return hash;

            return value switch
            {
                int v   => Add(hash, unchecked((uint)v)),
                bool v  => Add(hash, v ? 1u : 0u),
                Enum v  => Add(hash, unchecked((uint)Convert.ToInt32(v))),

                float v => Add(hash, Quantize(v, attr.Precision)),

                _ => throw new Exception(
                    $"Unsupported HashKey type: {value.GetType()}")
            };
        }
        
        private static object GetDefault(Type type)
            => type.IsValueType ? Activator.CreateInstance(type) : null;

        private static uint AddString(uint hash, string str)
        {
            if (str == null) return Add(hash, 0u);
            for (int i = 0; i < str.Length; i++)
                hash = Add(hash, str[i]);
            return hash;
        }
        
        private static uint Quantize(float v, int precision)
        {
            if (precision <= 0f)
                throw new Exception("Float HashKey requires precision");

            if (float.IsNaN(v) || float.IsInfinity(v))
                throw new Exception("Invalid float value for hashing");

            if (v == 0f) v = 0f;

            return unchecked((uint)Mathf.RoundToInt(v / Mathf.Pow(10, precision)));
        }

    }
}