using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Sackrany.Variables.Numerics
{
    [Serializable]
    public class LeveledVariable
    {
        [JsonProperty] public int Level { get; private set; }
        [JsonProperty] BigNumber _originalValue;

        [JsonIgnore] public int MaxLevel { get; private set; }
        [JsonIgnore] public bool IsReachedMaxLevel => Level >= MaxLevel;
        [JsonIgnore] public BigNumber Value
        {
            get
            {
                if (_isDirty) Recalculate();
                return _cachedValue;
            } 
        }

        [JsonIgnore] bool _isDirty = true;
        [JsonIgnore] BigNumber _cachedValue;
        [JsonIgnore] List<Func<BigNumber, BigNumber>> _continuousGrowthRules; 
        [JsonIgnore] List<Func<BigNumber, int, BigNumber>> _leveledGrowthRules; 
        
        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            _isDirty = true;
            _cachedValue = default;
            _continuousGrowthRules = new List<Func<BigNumber, BigNumber>>();
            _leveledGrowthRules = new List<Func<BigNumber, int, BigNumber>>();
        }
        
        public LeveledVariable(BigNumber value)
        {
            Level = 1;
            MaxLevel = -1;
            _originalValue = value;
            _continuousGrowthRules = new ();
            _leveledGrowthRules = new ();
        }

        public LeveledVariable DefaultLevel(int level)
        {
            if (Level == 1)
                Level = level;
            return this;
        }
        public LeveledVariable Max(int maxValue)
        {
            MaxLevel = maxValue;
            return this;
        }
        public LeveledVariable AddRule(Func<BigNumber, BigNumber> func)
        {
            _continuousGrowthRules.Add(func);
            return this;
        }
        public LeveledVariable AddRule(Func<BigNumber, int, BigNumber> func)
        {
            _leveledGrowthRules.Add(func);
            return this;
        }
        public LeveledVariable AddRule(IGrowthRule rule)
        {
            _continuousGrowthRules.Add(rule.Apply);
            return this;
        }

        public void ChangeLevel(int newLevel)
        {
            Level = newLevel;
            if (MaxLevel != -1) Level = Math.Min(MaxLevel, Level);
            if (Level < 0) Level = 0;
            _isDirty = true;
            OnLeveled?.Invoke(this);
        }
        public bool LevelUp(int amount = 1)
        {
            if (amount == 0) return false;
            if (MaxLevel != -1 && Level + amount > MaxLevel) return false;
            
            Level += amount;
            Level = Level < 0 ? 0 : Level;
            _isDirty = true;
            
            OnLeveled?.Invoke(this);
            return true;
        }
        public void Reset()
        {
            Level = 1;
            _cachedValue = _originalValue;
            _isDirty = false;
            OnReset?.Invoke(this);
        }
        public LeveledVariable Recalculate()
        {
            BigNumber result = _originalValue;
            for (var k = 1; k < Level; k++)
                for (var i = 0; i < _continuousGrowthRules.Count; i++)
                    result = _continuousGrowthRules[i](result);
            
            for (var i = 0; i < _leveledGrowthRules.Count; i++)
                result = _leveledGrowthRules[i](result, Level);
            
            _cachedValue = result;
            _isDirty = false;
            return this;
        }

        public BigNumber PotentialValue(int levelOffset = 1)
        {
            if (levelOffset == 0)
            {
                if (_isDirty) Recalculate();
                return _cachedValue;
            }
            
            BigNumber result = _originalValue;
            for (var k = 1; k < Level + levelOffset; k++)
                for (var i = 0; i < _continuousGrowthRules.Count; i++)
                    result = _continuousGrowthRules[i](result);
            
            for (var i = 0; i < _leveledGrowthRules.Count; i++)
                result = _leveledGrowthRules[i](result, Level + levelOffset);
            
            return result;
        }
        
        public static implicit operator BigNumber (LeveledVariable obj)
        {
            return obj.Value;
        }
        public static implicit operator LeveledVariable(BigNumber obj)
        {
            return new LeveledVariable(obj);
        }
        public static implicit operator LeveledVariable(float obj)
        {
            return new LeveledVariable(obj);
        }
        public static implicit operator LeveledVariable(int obj)
        {
            return new LeveledVariable(obj);
        }

        [field: NonSerialized] public event System.Action<LeveledVariable> OnLeveled;
        [field: NonSerialized] public event System.Action<LeveledVariable> OnReset;
    }
}