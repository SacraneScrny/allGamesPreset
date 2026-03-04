using Sackrany.Actor.DefaultFeatures.BehaviourFeature.Modules;
using Sackrany.Actor.Static;

namespace Sackrany.Actor.DefaultFeatures.BehaviourFeature
{
    public static class BehaviourChainExtensions
    {
        public static UnitChain IsAlive(this UnitChain chain)
            => chain.Where<HealthBehaviourModule>(m => !m.IsDead);

        public static UnitChain IsDead(this UnitChain chain)
            => chain.Where<HealthBehaviourModule>(m => m.IsDead);

        public static UnitChain IsGrounded(this UnitChain chain)
            => chain.Where<MovementBehaviourModule>(m => m.IsGrounded);
    }
}