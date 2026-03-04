using System;

using Sackrany.Variables.ExpandedVariable.Abstracts;
using Sackrany.Variables.Numerics;

namespace Sackrany.Variables.ExpandedVariable.Entities
{
    [Serializable]
    public class ExpandedBigNumber : BaseComplexityExpandedVariable<BigNumber>
    {
        public ExpandedBigNumber(BigNumber variable) : base(variable) { }
        private protected override BigNumber CalculateValue()
        {
            BigNumber _preadd = 0;
            for (var i = 0; i < BaseAdditional.Count; i++)
            {
                var a = BaseAdditional[i];
                _preadd += a.Invoke();
            }

            BigNumber _postadd = 0;
            for (var i = 0; i < PostAdditional.Count; i++)
            {
                var a = PostAdditional[i];
                _postadd += a.Invoke();
            }

            BigNumber _mult = 1;
            for (var i = 0; i < Multiply.Count; i++)
            {
                var a = Multiply[i];
                _mult *= a.Invoke();
            }

            return (Variable + _preadd) * _mult + _postadd;
        }
        
        
        public static implicit operator BigNumber (ExpandedBigNumber obj)
        {
            return obj.GetValue();
        }
        public static implicit operator ExpandedBigNumber(float value)
        {
            return new ExpandedBigNumber(value);
        }
        public static implicit operator ExpandedBigNumber(int value)
        {
            return new ExpandedBigNumber(value);
        }
        public static implicit operator ExpandedBigNumber(BigNumber value)
        {
            return new ExpandedBigNumber(value);
        }
    }
}