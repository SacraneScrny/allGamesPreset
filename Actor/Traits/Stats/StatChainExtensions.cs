using Sackrany.Actor.Static;

namespace Sackrany.Actor.Traits.Stats
{
    public static class StatChainExtensions
    {
        public static UnitChain HasStat<T>(this UnitChain chain) where T : IStat
            => chain.Where<StatHandlerModule>(m => m.HasStat<T>());

        public static UnitChain StatAbove<T>(this UnitChain chain, float threshold) where T : IStat
            => chain.Where<StatHandlerModule>(m => m.GetValue<T>() > threshold);

        public static UnitChain StatBelow<T>(this UnitChain chain, float threshold) where T : IStat
            => chain.Where<StatHandlerModule>(m => m.GetValue<T>() < threshold);
    }
}