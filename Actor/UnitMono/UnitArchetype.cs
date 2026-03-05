using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Sackrany.Actor.Static;

namespace Sackrany.Actor.UnitMono
{
    public readonly struct UnitArchetype : IEquatable<UnitArchetype>
    {
        public readonly uint Hash;

        public UnitArchetype(Unit unit)
        {
            Hash = ArchetypeCache.GetHash(unit);
        }

        public bool Equals(UnitArchetype other) => Hash == other.Hash;
        public override bool Equals(object obj) => obj is UnitArchetype other && Equals(other);
        public override int GetHashCode() => unchecked((int)Hash);
        public static bool operator ==(UnitArchetype l, UnitArchetype r) => l.Equals(r);
        public static bool operator !=(UnitArchetype l, UnitArchetype r) => !(l == r);
    }
}