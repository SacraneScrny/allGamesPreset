using System;

namespace Sackrany.SerializableData.Entities
{
    public class SerializeEntity<T> : ISerialize
    {
        private readonly Func<T> GetFunc;
        private readonly Action<T> SetFunc;
        
        public SerializeEntity(Func<T> _get, Action<T> _set)
        {
            GetFunc = _get;
            SetFunc = _set;
        }
        
        public object Get() => GetFunc.Invoke();
        public void Set(object obj) => SetFunc.Invoke((T)obj);
    }

    public interface ISerialize
    {
        public object Get();
        public void Set(object obj);
    }
}