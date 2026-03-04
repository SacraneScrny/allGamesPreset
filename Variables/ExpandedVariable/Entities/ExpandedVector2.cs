using System;

using Sackrany.Variables.ExpandedVariable.Abstracts;

using UnityEngine;

namespace Sackrany.Variables.ExpandedVariable.Entities
{
    [Serializable]
    public class ExpandedVector2 : BaseComplexityExpandedVariable<Vector2>
    {
        public ExpandedVector2(Vector2 variable) : base(variable) { }
        private protected override Vector2 CalculateValue()
        {
            Vector2 _preadd = Vector2.zero;
            for (var i = 0; i < BaseAdditional.Count; i++)
            {
                var a = BaseAdditional[i];
                _preadd += a.Invoke();
            }

            Vector2 _postadd = Vector2.zero;
            for (var i = 0; i < PostAdditional.Count; i++)
            {
                var a = PostAdditional[i];
                _postadd += a.Invoke();
            }

            Vector2 _mult = Vector2.one;
            for (var i = 0; i < Multiply.Count; i++)
            {
                var a = Multiply[i];
                var v = a.Invoke();
                _mult = new Vector2(_mult.x * v.x, _mult.y * v.y);
            }

            var v1 = (Variable + _preadd);
            v1 = new Vector2(v1.x * _mult.x, v1.y * _mult.y);
            v1 += _postadd;
            return v1;
        }

        public static implicit operator ExpandedVector2(Vector2 value)
        {
            return new ExpandedVector2(value);
        }
    }
}