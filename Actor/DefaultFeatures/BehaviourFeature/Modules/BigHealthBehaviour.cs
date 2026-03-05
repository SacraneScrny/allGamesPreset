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
    public class BigHealthBehaviourModule : Module, IDamageListener<BigDamageInfo>
    {
        bool _alreadyDied;
        [Template] BigHealthBehaviour _template;

        public BigNumber Health { get; protected set; }
        public ExpandedBigNumber MaxHealth { get; protected set; }
        
        protected override void OnAwake()
        {
            Health = _template.maxHealth;
            MaxHealth = _template.maxHealth;
        }
        protected override void OnStart()
        {
            MaxHealth.OnValueChanged += OnMaxHealthChanged;
        }
        
        void OnMaxHealthChanged(BigNumber value)
        {
            if (Health < 0) Health = 0;
        }
        void HealthChanged(BigNumber val)
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

        public void Damage(BigDamageInfo info)
        {
            Health -= info.Damage;
            if (Health < 0) Health = 0;
            HealthChanged(Health);
            if (info.Damage > BigNumber.Zero)
            {
                Unit.Event.Publish<OnDamage, BigDamageInfo>(info, true);
                OnDamaged?.Invoke(info);
            }
            else
            {
                Unit.Event.Publish<OnHeal, BigDamageInfo>(info, true);
                OnHealed?.Invoke(info);
            }
        }

        public void UpdateMaxHealth(BigNumber value)
        {
            if (_alreadyDied) return;
            float coef = MaxHealth.GetOriginalValue() / Health;
            MaxHealth.SetOriginalValue(value);
            Health = coef * value;
        }

        public bool IsDead => _alreadyDied;
        public Action OnDied;
        public Action<BigDamageInfo> OnDamaged;
        public Action<BigDamageInfo> OnHealed;
        public Action<BigNumber> OnHealthChanged;
        
        public void ProceedDamage(BigDamageInfo damage)
        {
            if (_alreadyDied) return;
            Damage(damage);
        }
    }
    
    [Serializable]
    public struct BigHealthBehaviour : ModuleTemplate<BigHealthBehaviourModule>
    {
        public BigNumber maxHealth;
    }
}