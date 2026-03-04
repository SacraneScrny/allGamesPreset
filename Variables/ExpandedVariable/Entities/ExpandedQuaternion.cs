using System;

using Sackrany.Variables.ExpandedVariable.Abstracts;

using UnityEngine;

namespace Sackrany.Variables.ExpandedVariable.Entities
{
    [Serializable]
    public class ExpandedQuaternion : BaseSimpleExpandedVariable<Quaternion>
    {
        public ExpandedQuaternion(Quaternion variable) : base(variable) { }
        private protected override Quaternion CalculateValue()
        {
            Quaternion _preadd = Quaternion.identity;
            for (var i = 0; i < BaseAdditional.Count; i++)
            {
                var a = BaseAdditional[i];
                _preadd *= a.Invoke();
            }

            return _preadd * Variable;
        }

        public static implicit operator ExpandedQuaternion(Quaternion value)
        {
            return new ExpandedQuaternion(value);
        }
    }
}