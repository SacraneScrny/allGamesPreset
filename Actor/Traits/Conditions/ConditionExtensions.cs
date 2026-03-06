using Sackrany.Actor.Static;
using Sackrany.Actor.UnitMono;

namespace Sackrany.Actor.Traits.Conditions
{
    public static class ConditionExtensions
    {
        public static bool IsAllowed<T>(this Unit unit) where T : ICondition
            => unit.Maybe<ConditionHandlerModule, bool>(h => h.IsAllowed<T>(), true);

        public static bool IsBlocked<T>(this Unit unit) where T : ICondition
            => !unit.IsAllowed<T>();

        public static bool Block<T>(this Unit unit, int amount = 1) where T : ICondition
            => unit.Maybe<ConditionHandlerModule>(h => h.Block<T>(amount));

        public static bool Unblock<T>(this Unit unit, int amount = 1) where T : ICondition
            => unit.Maybe<ConditionHandlerModule>(h => h.Unblock<T>(amount));

        public static bool UnblockAll<T>(this Unit unit) where T : ICondition
            => unit.Maybe<ConditionHandlerModule>(h => h.UnblockAll<T>());
    }
}