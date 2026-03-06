using System;

using Sackrany.Actor.Traits.Conditions.Static;

namespace Sackrany.Actor.Traits.Conditions
{
    public interface ICondition
    {
        int Id { get; }
    }
    [Serializable]
    public abstract class ACondition : ICondition
    {
        public abstract int Id { get; }
    }
    [Serializable]
    public abstract class ACondition<TSelf> : ACondition
        where TSelf : ACondition<TSelf>
    {
        public override int Id => ConditionRegistry.GetId<TSelf>();

        public sealed override bool Equals(object obj) => obj is ICondition other && other.Id == Id;
        public sealed override int GetHashCode() => Id.GetHashCode();
    }
    public readonly struct Condition<T> where T : ICondition
    {
        public static readonly int Id = ConditionRegistry.GetId<T>();
    }
}