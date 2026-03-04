using System;

using UnityEngine;

namespace Sackrany.Actor.Traits.Storage.DataBase
{
    [Serializable]
    public class SwordItem : ItemDefinition
    {
        [SerializeField] ItemConfig _config;
        public override ItemConfig Config => _config;
        
        [SerializeField] float _damage;
        public float Damage => _damage;
    }

    [Serializable]
    public class HealthPotion : ItemDefinition
    {
        [SerializeField] ItemConfig _config;
        public override ItemConfig Config => _config;
        
        [SerializeField] float _healAmount;
        public float HealAmount => _healAmount;
    }
}