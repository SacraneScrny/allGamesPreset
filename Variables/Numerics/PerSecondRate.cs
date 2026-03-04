using System;

using UnityEngine;

namespace Sackrany.Variables.Numerics
{
    public class PerSecondRate<T> where T : unmanaged
    {
        readonly Func<T> _getter;
        readonly Func<T, T, BigNumber> _delta;

        T _prev;
        float _prevTime;
        bool _hasPrev;

        public BigNumber Current { get; private set; }

        public PerSecondRate(
            Func<T> getter,
            Func<T, T, BigNumber> delta)
        {
            _getter = getter;
            _delta = delta;
        }

        public void Update(float smooth = 0.15f)
        {
            var now = Time.timeSinceLevelLoad;
            var value = _getter();

            if (_hasPrev)
            {
                var dt = now - _prevTime;
                if (dt > 0f)
                {
                    var raw = _delta(value, _prev) / dt;
                    Current = Current.Lerp(raw, smooth);
                }
            }

            _prev = value;
            _prevTime = now;
            _hasPrev = true;
        }
    }
}