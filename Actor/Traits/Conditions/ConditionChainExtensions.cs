using Sackrany.Actor.Static;

namespace Sackrany.Actor.Traits.Conditions
{
    public static class ConditionChainExtensions
    {
        public static UnitChain IsAllowed<T>(this UnitChain chain) where T : ICondition
            => chain.Where<ConditionHandlerModule>(m => m.IsAllowed<T>());

        public static UnitChain IsBlocked<T>(this UnitChain chain) where T : ICondition
            => chain.Where<ConditionHandlerModule>(m => m.IsBlocked<T>());
    }
}