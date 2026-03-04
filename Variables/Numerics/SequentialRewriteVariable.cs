using System;
using System.Collections.Generic;

using UnityEngine;

namespace Sackrany.Variables.Numerics
{
    public class SequentialRewriteVariable<T>
    {
        public delegate T sequentialDelegate(T value);
        readonly List<sequentialDelegate> delegates = new ();
        int _cachedFrame = -1;
        T _cachedValue;

        public sequentialDelegate Subscribe(sequentialDelegate del)
        {
            delegates.Add(del);
            return del;
        }        
        public sequentialDelegate Subscribe(Func<T, T> del)
        {
            sequentialDelegate ret = (x) => del(x);
            delegates.Add(ret);
            return ret;
        }
        public bool Unsubscribe(sequentialDelegate del)
        {
            return delegates.Remove(del);
        }
        
        public T Calculate(T from)
        {
            if (_cachedFrame == Time.frameCount)
                return _cachedValue;
            
            var result = from;
            for (int i = 0; i < delegates.Count; i++)
                result = delegates[i](result);
            
            _cachedValue = result;
            _cachedFrame = Time.frameCount;
            return result;
        }
        public T Calculate() => Calculate(default(T));

        public void Clear()
        {
            delegates.Clear();
            _cachedFrame = -1;
            _cachedValue = default;
        }
    }
}