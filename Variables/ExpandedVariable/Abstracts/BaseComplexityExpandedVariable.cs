using System;
using System.Collections.Generic;

namespace Sackrany.Variables.ExpandedVariable.Abstracts
{
    [Serializable]
    public abstract class BaseComplexityExpandedVariable<T> : BaseExpandedVariable<T>
    {
        private protected List<expandedDelegate> BaseAdditional = new();
        private protected List<expandedDelegate> Multiply = new();
        private protected List<expandedDelegate> PostAdditional = new();
        
        protected BaseComplexityExpandedVariable(T variable) : base(variable)
        {
            Preload();
        }
        
        private protected sealed override void Preload()
        {
            BaseAdditional ??= new List<expandedDelegate>();
            Multiply ??= new List<expandedDelegate>();
            PostAdditional ??= new List<expandedDelegate>();
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

        public expandedDelegate Add_Multiply(Func<T> func)
        {
            expandedDelegate ret = () => func();
            Multiply.Add(ret);
            return ret;
        }
        public expandedDelegate Add_Multiply(T rawValue)
        {
            expandedDelegate ret = () => rawValue;
            Multiply.Add(ret);
            return ret;
        }
        public bool Remove_Multiply(expandedDelegate func)
        {
            var ret = Multiply.Remove(func);
            return ret;
        }

        public expandedDelegate Add_PostAdditional(Func<T> func)
        {
            expandedDelegate ret = () => func();
            PostAdditional.Add(ret);
            return ret;
        }    
        public expandedDelegate Add_PostAdditional(T rawValue)
        {
            expandedDelegate ret = () => rawValue;
            PostAdditional.Add(ret);
            return ret;
        }       
        public bool Remove_PostAdditional(expandedDelegate func)
        {
            var ret = PostAdditional.Remove(func);
            return ret;
        }
        private protected override void OnClear()
        {
            BaseAdditional.Clear();
            Multiply.Clear();
            PostAdditional.Clear();
        }
        
        public override object Clone()
        {
            var clone = (BaseComplexityExpandedVariable<T>) MemberwiseClone();

            Preload();
            clone.Preload();
            
            clone.BaseAdditional = new List<expandedDelegate>(this.BaseAdditional);
            clone.Multiply = new List<expandedDelegate>(this.Multiply);
            clone.PostAdditional = new List<expandedDelegate>(this.PostAdditional);

            clone.HandleCloned(clone);
            return clone;
        }
        protected virtual void HandleCloned(BaseComplexityExpandedVariable<T> clone) { }
        
        public int Count => BaseAdditional.Count + Multiply.Count + PostAdditional.Count;
    }
}