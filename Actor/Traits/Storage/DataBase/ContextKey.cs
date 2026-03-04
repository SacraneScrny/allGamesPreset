using System;

namespace Sackrany.Actor.Traits.Storage.DataBase
{
    public readonly struct ContextKey : IEquatable<ContextKey>
    {
        public readonly int TypeId;
        public readonly uint InstanceId;

        public ContextKey(int typeId, uint instanceId = 0)
        {
            TypeId = typeId;
            InstanceId = instanceId;
        }

        public bool Equals(ContextKey other) => TypeId == other.TypeId && InstanceId == other.InstanceId;
        public override bool Equals(object obj) => obj is ContextKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(TypeId, InstanceId);
    }
}