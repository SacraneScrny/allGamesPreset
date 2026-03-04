using System;

using Sackrany.Actor.EventBus;
using Sackrany.Actor.Modules;
using Sackrany.Actor.Modules.Modules;
using Sackrany.Actor.Static;
using Sackrany.Variables.Numerics;

namespace Sackrany.Actor.Traits.Damage
{
    public class DamageListenerModule<T> : Module
        where T : struct
    {
        [Dependency] protected IDamageListener<T>[] _damageListeners;
        public readonly SequentialRewriteVariable<T> DamageOverwrite = new ();
        
        protected override void OnStart()
        {
            Unit.Event.Subscribe<OnDamage, T>(ProcessDamage);
        }
        public void ProcessDamage(T damage)
        {
            damage = ProcessDamage_Internal(damage);
            OnDamage?.Invoke(damage);
            Unit.Event.Publish<OnDamagePostProcess, T>(damage);
        }
        T ProcessDamage_Internal(T damage)
        {
            var newDmg = DamageOverwrite.Calculate(damage);
            for (int i = 0; i < _damageListeners.Length; i++)
                _damageListeners[i].ProceedDamage(newDmg);
            return newDmg;
        }
        
        protected sealed override void OnReset()
        {
            OnDamage = null;
            DamageOverwrite.Clear();
        }
        
        public delegate void DamageHandler(T damage);
        public event DamageHandler OnDamage;
    }
    public interface IDamageListener<in T>
        where T : struct
    {
        public void ProceedDamage(T damage);
    }

    [Serializable]
    public struct DamageListener : ModuleTemplate
    {
        [HashKey] public DamageType listenerType;

        public int GetId()
            => listenerType switch
            {
                DamageType.BigDamageInfo => ModuleRegistry.GetId<DamageListenerModule<BigDamageInfo>>(),
                _ => ModuleRegistry.GetId<DamageListenerModule<DamageInfo>>()
            };
        public Module GetInstance()
            => listenerType switch
            {
                DamageType.BigDamageInfo => new DamageListenerModule<BigDamageInfo>(),
                _ => new DamageListenerModule<DamageInfo>(),
            };
    }
}