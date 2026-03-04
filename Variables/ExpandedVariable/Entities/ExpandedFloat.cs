using System;

using Sackrany.Variables.ExpandedVariable.Abstracts;

namespace Sackrany.Variables.ExpandedVariable.Entities
{
    [Serializable]
    public class ExpandedFloat : BaseComplexityExpandedVariable<float>
    {
        public ExpandedFloat(float variable) : base(variable) { }
        private protected override float CalculateValue()
        {
            float _preadd = 0;
            for (var i = 0; i < BaseAdditional.Count; i++)
            {
                var a = BaseAdditional[i];
                _preadd += a.Invoke();
            }

            float _postadd = 0;
            for (var i = 0; i < PostAdditional.Count; i++)
            {
                var a = PostAdditional[i];
                _postadd += a.Invoke();
            }

            float _mult = 1;
            for (var i = 0; i < Multiply.Count; i++)
            {
                var a = Multiply[i];
                _mult *= a.Invoke();
            }

            return (Variable + _preadd) * _mult + _postadd;
        }

        public static implicit operator ExpandedFloat(float value)
        {
            return new ExpandedFloat(value);
        }
    }
}