using System;

using Sackrany.Variables.ExpandedVariable.Abstracts;

namespace Sackrany.Variables.ExpandedVariable.Entities
{
    [Serializable]
    public class ExpandedInt : BaseComplexityExpandedVariable<int>
    {
        public ExpandedInt(int variable) : base(variable) { }
        private protected override int CalculateValue()
        {
            int _preadd = 0;
            for (var i = 0; i < BaseAdditional.Count; i++)
            {
                var a = BaseAdditional[i];
                _preadd += a.Invoke();
            }

            int _postadd = 0;
            for (var i = 0; i < PostAdditional.Count; i++)
            {
                var a = PostAdditional[i];
                _postadd += a.Invoke();
            }

            int _mult = 1;
            for (var i = 0; i < Multiply.Count; i++)
            {
                var a = Multiply[i];
                _mult *= a.Invoke();
            }

            return (Variable + _preadd) * _mult + _postadd;
        }

        public static implicit operator ExpandedInt(int value)
        {
            return new ExpandedInt(value);
        }
    }
}