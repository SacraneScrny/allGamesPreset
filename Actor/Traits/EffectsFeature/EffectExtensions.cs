using Sackrany.Actor.Static;
using Sackrany.Actor.UnitMono;

namespace Sackrany.Actor.Traits.EffectsFeature
{
    public static class EffectExtensions
    {        
        public static bool ApplyEffects(this Unit unit, EffectTemplate[] effects)
            => unit.Maybe<EffectHandlerModule>(h => h.ApplyEffects(effects));
        public static bool ApplyEffect<T>(this Unit unit, int amount = 1) where T : EffectTemplate, new ()
            => unit.Maybe<EffectHandlerModule>(h => h.ApplyEffect<T>(amount));
        public static bool ApplyEffect<T>(this Unit unit, T effect, int amount = 1) where T : EffectTemplate
            => unit.Maybe<EffectHandlerModule>(h => h.ApplyEffect(effect, amount));

        public static bool RemoveEffect<T>(this Unit unit) where T : Effect
            => unit.Maybe<EffectHandlerModule>(h => h.RemoveEffect<T>());
        public static bool RemoveEffect<T>(this Unit unit, T effect) where T : Effect 
            => unit.Maybe<EffectHandlerModule>(h => h.RemoveEffect<T>(effect));
        public static bool RemoveAllEffects(this Unit unit)
            => unit.Maybe<EffectHandlerModule>(h => h.RemoveAllEffects());

        public static bool ChangeEffectAmount<T>(this Unit unit, int offset) where T : Effect
            => unit.Maybe<EffectHandlerModule>(h => h.ChangeEffectAmount<T>(offset));
        public static bool ChangeEffectAmount<T>(this Unit unit, T effect, int offset) where T : Effect 
            => unit.Maybe<EffectHandlerModule>(h => h.ChangeEffectAmount<T>(effect, offset));
    }
}