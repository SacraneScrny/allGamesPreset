using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Sackrany.Actor.Static;

namespace Sackrany.Actor.Unit
{
    public readonly struct UnitArchetype : IEquatable<UnitArchetype>
    {
        public readonly uint Hash;

        public UnitArchetype(Unit unit)
        {
            Hash = HashBuilder.BuildFromTemplates(CollectTemplates(unit));
        }
        static List<object> CollectTemplates(Unit unit)
        {
            var list = new List<object>(16);
            var controller = unit.GetType().GetField(
                "Controller",
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            )
            ?.GetValue(unit);
            var type = controller.GetType();

            var field = type.GetField(
                "Default",
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

            if (field == null) return list;

            if (field.GetValue(controller) is IEnumerable templates)
            {
                foreach (var t in templates)
                    if (t != null)
                        list.Add(t);
            }

            return list;
        }

        public bool Equals(UnitArchetype other)
        {
            return Hash == other.Hash;
        }
        public override bool Equals(object obj)
            => obj is UnitArchetype other && Equals(other);

        public override int GetHashCode()
            => unchecked((int)Hash);

        public static bool operator ==(UnitArchetype left, UnitArchetype right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(UnitArchetype left, UnitArchetype right)
            => !(left == right);
    }
}