using System;

using Sackrany.Actor.EventBus;
using Sackrany.Actor.Modules;
using Sackrany.Actor.Modules.Modules;
using Sackrany.Actor.Traits.Damage;
using Sackrany.Actor.UnitMono;
using Sackrany.Variables.ExpandedVariable.Entities;
using Sackrany.Variables.Numerics;

namespace Sackrany.Actor.DefaultFeatures.BehaviourFeature.Modules
{
    public class HealthBehaviourModule : Module, IDamageListener<DamageInfo>
    {
        bool _alreadyDied;
        [Template] HealthBehaviour _template;

        public float Health { get; protected set; }
        public ExpandedFloat MaxHealth { get; protected set; }
        
        protected override void OnAwake()
        {
            Health = _template.maxHealth;
            MaxHealth = _template.maxHealth;
        }
        protected override void OnStart()
        {
            MaxHealth.OnValueChanged += OnMaxHealthChanged;
        }
        
        void OnMaxHealthChanged(float value)
        {
            if (Health < 0) Health = 0;
        }
        void HealthChanged(float val)
        {
            OnHealthChanged?.Invoke(Health);
            if (_alreadyDied || val > 0) return;
            Unit.Event.Publish<OnDied, Unit>(Unit, true);
            OnDied?.Invoke();
            _alreadyDied = true;
        }
        protected override void OnReset()
        {
            MaxHealth.Clear();
            Health = _template.maxHealth;
            _alreadyDied = false;
            OnDied = null;
            OnHealed = null;
            OnDamaged = null;
            OnHealthChanged = null;
        }

        public void Damage(DamageInfo info)
        {
            Health -= info.Damage;
            if (Health < 0) Health = 0;
            HealthChanged(Health);
            if (info.Damage > BigNumber.Zero)
            {
                Unit.Event.Publish<OnDamage, DamageInfo>(info, true);
                OnDamaged?.Invoke(info);
            }
            else
            {
                Unit.Event.Publish<OnHeal, DamageInfo>(info, true);
                OnHealed?.Invoke(info);
            }
        }

        public void UpdateMaxHealth(float value)
        {
            if (_alreadyDied) return;
            float coef = MaxHealth.GetOriginalValue() / Health;
            MaxHealth.SetOriginalValue(value);
            Health = coef * value;
        }

        public bool IsDead => _alreadyDied;
        public Action OnDied;
        public Action<DamageInfo> OnDamaged;
        public Action<DamageInfo> OnHealed;
        public Action<float> OnHealthChanged;
        
        public void ProceedDamage(DamageInfo damage)
        {
            if (_alreadyDied) return;
            Damage(damage);
        }
    }
    
    [Serializable]
    public struct HealthBehaviour : ModuleTemplate<HealthBehaviourModule>
    {
        public float maxHealth;
    }
}