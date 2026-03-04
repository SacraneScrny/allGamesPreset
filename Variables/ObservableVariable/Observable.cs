using System;
using System.Collections.Generic;

using UnityEngine;

namespace Sackrany.Variables.ObservableVariable
{
    public class Observable<T> : IDisposable
    {
        public delegate void observerDelegate(T obj);
        
        [SerializeField] T _value;
        [SerializeField] T _cache;
        List<observerDelegate> _onChanged;
        readonly IEqualityComparer<T> _comparer;
        bool _disposed;

        public Observable(IEqualityComparer<T> comparer = null)
        {
            _comparer = comparer ?? EqualityComparer<T>.Default;
        }

        public T Value
        {
            get => _value;
            set => Set(value);
        }
        void Set(T value)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Observable<T>));
            _value = value;
            if (_comparer.Equals(_value, _cache))
                return;
            _cache = value;
            Invoke();
        }
        void Invoke()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Observable<T>));
            if (_onChanged == null || _onChanged.Count == 0)
                return;
            
            var snapshot = _onChanged.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
                snapshot[i]?.Invoke(_value);
        }

        public IDisposable Subscribe(observerDelegate callback, bool invokeImmediately = false)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Observable<T>));
            if (callback == null) return null;
            if (_onChanged == null) _onChanged = new ();
            _onChanged.Add(callback);
            if (invokeImmediately)
                callback(_value);
            return new Subscription(() => Unsubscribe(callback));
        }
        public bool Unsubscribe(observerDelegate callback)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Observable<T>));
            if (_onChanged == null) return false;
            return callback != null && _onChanged.Remove(callback);
        }
        public void UnsubscribeAll()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Observable<T>));
            if (_onChanged == null) return;
            _onChanged.Clear();
        }
        
        public Observable<T> Derive(IEqualityComparer<T> comparer = null)
        {
            var derived = new Observable<T>(comparer);

            Subscribe(
                new observerDelegate(v => derived.Value = v),
                invokeImmediately: true
            );

            return derived;
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            UnsubscribeAll();
            _value = default;
            _cache = default;
            _disposed = true;
        }
        
        public static implicit operator T(Observable<T> observable)
        {
            if (observable == null)
                throw new NullReferenceException(
                    $"Implicit conversion from null Observable<{typeof(T).Name}>");

            return observable.Value;
        }
    }
    internal sealed class Subscription : IDisposable
    {
        Action _unsubscribe;
        bool _disposed;

        public Subscription(Action unsubscribe)
            => _unsubscribe = unsubscribe;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _unsubscribe?.Invoke();
            _unsubscribe = null;
        }
    }
}