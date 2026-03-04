using System;
using System.Collections.Generic;

using UnityEngine;

namespace Sackrany.Variables.ObservableVariable
{
    public sealed class FloatComparer : IEqualityComparer<float>
    {
        readonly float _eps;
        public FloatComparer(float eps = 0.0001f) => _eps = eps;

        public bool Equals(float a, float b)
            => Mathf.Abs(a - b) <= _eps;

        public int GetHashCode(float obj) => 0;
    }
    public sealed class DoubleComparer : IEqualityComparer<double>
    {
        readonly double _eps;
        public DoubleComparer(double eps = 0.0000001) => _eps = eps;

        public bool Equals(double a, double b)
            => Math.Abs(a - b) <= _eps;

        public int GetHashCode(double obj) => 0;
    }
    public sealed class StringComparerOrdinal : IEqualityComparer<string>
    {
        public bool Equals(string a, string b)
            => string.Equals(a, b, StringComparison.Ordinal);

        public int GetHashCode(string obj)
            => obj?.GetHashCode() ?? 0;
    }
    public sealed class Vector2Comparer : IEqualityComparer<Vector2>
    {
        readonly float _eps;
        public Vector2Comparer(float eps = 0.0001f) => _eps = eps;

        public bool Equals(Vector2 a, Vector2 b)
            => (a - b).sqrMagnitude <= _eps * _eps;

        public int GetHashCode(Vector2 obj) => 0;
    }
    public sealed class Vector3Comparer : IEqualityComparer<Vector3>
    {
        readonly float _eps;
        public Vector3Comparer(float eps = 0.0001f) => _eps = eps;

        public bool Equals(Vector3 a, Vector3 b)
            => (a - b).sqrMagnitude <= _eps * _eps;

        public int GetHashCode(Vector3 obj) => 0;
    }
    public sealed class Vector4Comparer : IEqualityComparer<Vector4>
    {
        readonly float _eps;
        public Vector4Comparer(float eps = 0.0001f) => _eps = eps;

        public bool Equals(Vector4 a, Vector4 b)
            => (a - b).sqrMagnitude <= _eps * _eps;

        public int GetHashCode(Vector4 obj) => 0;
    }
    public sealed class QuaternionComparer : IEqualityComparer<Quaternion>
    {
        readonly float _eps;
        public QuaternionComparer(float eps = 0.0001f) => _eps = eps;

        public bool Equals(Quaternion a, Quaternion b)
            => Quaternion.Angle(a, b) <= _eps;

        public int GetHashCode(Quaternion obj) => 0;
    }
    public sealed class ColorComparer : IEqualityComparer<Color>
    {
        readonly float _eps;
        public ColorComparer(float eps = 0.001f) => _eps = eps;

        public bool Equals(Color a, Color b)
            => Mathf.Abs(a.r - b.r) <= _eps
               && Mathf.Abs(a.g - b.g) <= _eps
               && Mathf.Abs(a.b - b.b) <= _eps
               && Mathf.Abs(a.a - b.a) <= _eps;

        public int GetHashCode(Color obj) => 0;
    }
    public sealed class BoundsComparer : IEqualityComparer<Bounds>
    {
        readonly float _eps;
        public BoundsComparer(float eps = 0.0001f) => _eps = eps;

        public bool Equals(Bounds a, Bounds b)
            => (a.center - b.center).sqrMagnitude <= _eps * _eps
               && (a.size - b.size).sqrMagnitude <= _eps * _eps;

        public int GetHashCode(Bounds obj) => 0;
    }
    public sealed class RectComparer : IEqualityComparer<Rect>
    {
        readonly float _eps;
        public RectComparer(float eps = 0.0001f) => _eps = eps;

        public bool Equals(Rect a, Rect b)
            => Mathf.Abs(a.x - b.x) <= _eps
               && Mathf.Abs(a.y - b.y) <= _eps
               && Mathf.Abs(a.width - b.width) <= _eps
               && Mathf.Abs(a.height - b.height) <= _eps;

        public int GetHashCode(Rect obj) => 0;
    }
}