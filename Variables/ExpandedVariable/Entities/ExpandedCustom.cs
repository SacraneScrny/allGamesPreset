using Sackrany.Variables.ExpandedVariable.Abstracts;

namespace Sackrany.Variables.ExpandedVariable.Entities
{
    public class ExpandedCustom<TVar> : BaseComplexityExpandedVariable<TVar>
        where TVar : struct, ICustomVariable<TVar>
    {
        public ExpandedCustom(TVar variable) : base(variable) { }
        
        private protected override TVar CalculateValue()
        {
            var baseVar = Variable;
            for (var i = 0; i < BaseAdditional.Count; i++)
            {
                var a = BaseAdditional[i];
                baseVar.Add(a.Invoke());
            }
            
            for (var i = 0; i < Multiply.Count; i++)
            {
                var a = Multiply[i];
                baseVar.Multiply(a.Invoke());
            }
            
            for (var i = 0; i < PostAdditional.Count; i++)
            {
                var a = PostAdditional[i];
                baseVar.Add(a.Invoke());
            }

            return baseVar;
        }
        
        public static implicit operator ExpandedCustom<TVar>(TVar value)
        {
            return new ExpandedCustom<TVar>(value);
        }
    }

    public interface ICustomVariable<in TSelf> where TSelf : ICustomVariable<TSelf>
    {
        public void Add(TSelf variable);
        public void Multiply(TSelf variable);
    }

    public struct FloatCustom : ICustomVariable<FloatCustom>
    {
        public float Value { get; set; }
        public void Add(FloatCustom variable)
        {
            Value += variable.Value;
        }
        public void Multiply(FloatCustom variable)
        {
            Value *= variable.Value;
        }
    }
}