using System;
using System.Collections.Generic;

namespace Sackrany.Variables.ExpandedVariable.Abstracts
{
    public abstract class BaseSimpleExpandedVariable<T> : BaseExpandedVariable<T>
    {
        private protected List<expandedDelegate> BaseAdditional = new();
        
        protected BaseSimpleExpandedVariable(T variable) : base(variable)
        {
            Preload();
        }
        
        private protected sealed override void Preload()
        {
            BaseAdditional ??= new List<expandedDelegate>();
        }
        
        public expandedDelegate Add_BaseAdditional(Func<T> func)
        {
            expandedDelegate ret = () => func();
            BaseAdditional.Add(ret);
            return ret;
        }    
        public expandedDelegate Add_BaseAdditional(T rawValue)
        {
            expandedDelegate ret = () => rawValue;
            BaseAdditional.Add(ret);
            return ret;
        }       
        public bool Remove_BaseAdditional(expandedDelegate func)
        {
            var ret = BaseAdditional.Remove(func);
            return ret;
        }
        private protected override void OnClear()
        {
            BaseAdditional.Clear();
        }
        
        public override object Clone()
        {
            var clone = (BaseSimpleExpandedVariable<T>)MemberwiseClone();

            Preload();

            clone.Preload();
            clone.BaseAdditional.AddRange(BaseAdditional.ToArray());

            HandleCloned(clone);
            return clone;
        }
        protected virtual void HandleCloned(BaseSimpleExpandedVariable<T> clone) { }
    }
}