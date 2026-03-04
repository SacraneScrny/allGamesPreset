using System;

using Sackrany.Variables.ExpandedVariable.Abstracts;

using UnityEngine;

namespace Sackrany.Variables.ExpandedVariable.Entities
{
    [Serializable]
    public class ExpandedVector3 : BaseComplexityExpandedVariable<Vector3>
    {
        public ExpandedVector3(Vector3 variable) : base(variable) { }
        private protected override Vector3 CalculateValue()
        {
            Vector3 _preadd = Vector3.zero;
            for (var i = 0; i < BaseAdditional.Count; i++)
            {
                var a = BaseAdditional[i];
                _preadd += a.Invoke();
            }

            Vector3 _postadd = Vector3.zero;
            for (var i = 0; i < PostAdditional.Count; i++)
            {
                var a = PostAdditional[i];
                _postadd += a.Invoke();
            }

            Vector3 _mult = Vector3.one;
            for (var i = 0; i < Multiply.Count; i++)
            {
                var a = Multiply[i];
                var v = a.Invoke();
                _mult = new Vector3(_mult.x * v.x, _mult.y * v.y, _mult.z * v.z);
            }

            var v1 = (Variable + _preadd);
            v1 = new Vector3(v1.x * _mult.x, v1.y * _mult.y, v1.z * _mult.z);
            v1 += _postadd;
            return v1;
        }

        public static implicit operator ExpandedVector3(Vector3 value)
        {
            return new ExpandedVector3(value);
        }
    }
}