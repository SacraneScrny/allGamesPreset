using System;

using Sackrany.Variables.ExpandedVariable.Abstracts;

namespace Sackrany.Variables.ExpandedVariable.Entities
{
    [Serializable]
    public class ExpandedBool : BaseComplexityExpandedVariable<bool>
    {
        public ExpandedBool(bool variable) : base(variable) { }
        private protected override bool CalculateValue()
        {
            bool _preadd = false;
            for (var i = 0; i < BaseAdditional.Count; i++)
            {
                var a = BaseAdditional[i];
                _preadd |= a.Invoke();
            }

            bool _postadd = false;
            for (var i = 0; i < PostAdditional.Count; i++)
            {
                var a = PostAdditional[i];
                _postadd |= a.Invoke();
            }

            bool _mult = true;
            for (var i = 0; i < Multiply.Count; i++)
            {
                var a = Multiply[i];
                _mult &= a.Invoke();
            }

            return (Variable | _preadd) & _mult | _postadd;
        }

        public static implicit operator ExpandedBool(bool value)
        {
            return new ExpandedBool(value);
        }
    }
}