using Sackrany.Actor.Static;

namespace Sackrany.Actor.Traits.EffectsFeature
{
    public static class EffectChainExtensions
    {
        public static UnitChain HasEffect<TEffect>(this UnitChain chain) where TEffect : Effect
            => chain.Where<EffectHandlerModule>(m => m.HasEffect<TEffect>());
    }
}