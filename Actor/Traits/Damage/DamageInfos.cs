using JetBrains.Annotations;

using Sackrany.Actor.Traits.Affinity;
using Sackrany.Actor.UnitMono;
using Sackrany.Variables.Numerics;

using UnityEngine;

namespace Sackrany.Actor.Traits.Damage
{
    public interface IDamage { }
    public interface IDamage<out T> : IDamage
        where T : struct
    {
        T Damage { get; }
        Vector3 Direction { get; }
        Vector3 HitPosition { get; }
        Vector3 AttackPosition { get; }
        IAffinity Affinity { get; }
        
        Unit Attacker { get; }
        Unit Target { get; }

        bool HitSomebody();
        bool HitSelf();
        bool FriendlyFire();
    }
    
    public readonly struct BigDamageInfo : IDamage<BigNumber>
    {
        public BigNumber Damage { get; }
        public Vector3 Direction { get; }
        public Vector3 HitPosition { get; }
        public Vector3 AttackPosition { get; }
        public IAffinity Affinity { get; }

        public Unit Attacker { get; }
        public Unit Target { get; }
        
        public bool HitSomebody() => Target != null;
        public bool HitSelf() => HitSomebody() && Attacker == Target;
        public bool FriendlyFire() => HitSomebody() && Attacker.Team == Target.Team;

        public BigDamageInfo(
            BigNumber damage,
            Vector3? hitPosition,
            Vector3? attackPosition,
            [CanBeNull] Unit attacker,
            [CanBeNull] Unit target,
            [CanBeNull] IAffinity affinity)
        {
            Damage = damage;
            
            Attacker = attacker;
            Target = target;
            
            if (hitPosition.HasValue)
                HitPosition = hitPosition.Value;
            else if (target != null) HitPosition = target.transform.position;
            else if (attacker != null) HitPosition = attacker.transform.position;
            else HitPosition = Vector3.zero;
            
            if (attackPosition.HasValue)
                AttackPosition = attackPosition.Value;
            else if (attacker != null) AttackPosition = attacker.transform.position;
            else if (target != null) AttackPosition = target.transform.position;
            else AttackPosition = Vector3.zero;

            Direction = HitPosition - AttackPosition;
            Affinity = affinity ?? Affinity<Default>.Instance;
        }
    }
    public readonly struct DamageInfo : IDamage<float>
    {
        public float Damage { get; }
        public Vector3 Direction { get; }
        public Vector3 HitPosition { get; }
        public Vector3 AttackPosition { get; }
        public IAffinity Affinity { get; }

        public Unit Attacker { get; }
        public Unit Target { get; }
        
        public bool HitSomebody() => Target != null;
        public bool HitSelf() => HitSomebody() && Attacker == Target;
        public bool FriendlyFire() => HitSomebody() && Attacker.Team == Target.Team;

        public DamageInfo(
            float damage,
            Vector3? hitPosition,
            Vector3? attackPosition,
            [CanBeNull] Unit attacker,
            [CanBeNull] Unit target,
            [CanBeNull] IAffinity affinity)
        {
            Damage = damage;
            
            Attacker = attacker;
            Target = target;
            
            if (hitPosition.HasValue)
                HitPosition = hitPosition.Value;
            else if (target != null) HitPosition = target.transform.position;
            else if (attacker != null) HitPosition = attacker.transform.position;
            else HitPosition = Vector3.zero;
            
            if (attackPosition.HasValue)
                AttackPosition = attackPosition.Value;
            else if (attacker != null) AttackPosition = attacker.transform.position;
            else if (target != null) AttackPosition = target.transform.position;
            else AttackPosition = Vector3.zero;

            Direction = HitPosition - AttackPosition;
            Affinity = affinity ?? Affinity<Default>.Instance;
        }
    }

    public enum DamageType
    {
        BigDamageInfo,
        DamageInfo
    }
}