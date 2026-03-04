using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Sackrany.Variables.Numerics
{
    [Serializable]
    public class GrowthVariable
    {
        [JsonProperty] private BigNumber _originalValue;
        [JsonProperty] public BigNumber Value { get; private set; }
        [JsonIgnore] private List<Func<BigNumber, BigNumber>> _growthRules; 

        public GrowthVariable(BigNumber value)
        {
            _originalValue = value;
            Value = value;
            _growthRules = new ();
        }
        
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _growthRules = new List<Func<BigNumber, BigNumber>>();
        }
        
        public GrowthVariable AddRule(Func<BigNumber, BigNumber> func)
        {
            _growthRules.Add(func);
            return this;
        }
        public GrowthVariable AddRule(IGrowthRule rule)
        {
            _growthRules.Add(rule.Apply);
            return this;
        }

        public void Reset()
        {
            Value = _originalValue;
            OnReset?.Invoke(this);
        }
        public GrowthVariable Growth(int count = 1)
        {
            for (var k = 0; k < count; k++)
                for (var i = 0; i < _growthRules.Count; i++)
                    Value = _growthRules[i](Value);
            OnGrowth?.Invoke(this);
            return this;
        }
        public BigNumber PotentialGrowth(int count = 1)
        {
            BigNumber result = Value;
            for (var k = 0; k < count; k++)
                for (var i = 0; i < _growthRules.Count; i++)
                    result = _growthRules[i](result);
            return result;
        }
        
        public static implicit operator BigNumber (GrowthVariable obj)
        {
            return obj.Value;
        }
        public static implicit operator GrowthVariable(BigNumber obj)
        {
            return new GrowthVariable(obj);
        }
        public static implicit operator GrowthVariable(float obj)
        {
            return new GrowthVariable(obj);
        }
        public static implicit operator GrowthVariable(int obj)
        {
            return new GrowthVariable(obj);
        }
        
        [field: NonSerialized] public event System.Action<GrowthVariable> OnGrowth; 
        [field: NonSerialized] public event System.Action<GrowthVariable> OnReset; 
    }

    public interface IGrowthRule
    {
        public BigNumber Apply(BigNumber value);
    }
}