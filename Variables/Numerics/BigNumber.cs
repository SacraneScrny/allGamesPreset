using System;
using System.Collections.Generic;
using System.Globalization;

using Newtonsoft.Json;

using Sackrany.Variables.ExpandedVariable.Entities;

using Unity.Mathematics;

using UnityEngine;

namespace Sackrany.Variables.Numerics
{
    [Serializable]
    public struct BigNumber : IComparable<BigNumber>, IEquatable<BigNumber>, ICustomVariable<BigNumber>
    {
        public static readonly BigNumber Epsilon = new BigNumber(Mathf.Epsilon);
        public static readonly BigNumber Zero = new BigNumber(0);
        public static readonly BigNumber One = new BigNumber(1);
        
        [JsonProperty] [SerializeField] double mantissa;
        [JsonProperty] [SerializeField] int exponent;

        [JsonIgnore] public double Mantissa => mantissa;
        [JsonIgnore] public int Exponent => exponent;
        
        static double[] _powerOfTenCache = InitCache();
        const int MinCachedExponent = -30;
        const int MaxCachedExponent = 30;
        static double[] InitCache()
        {
            int size = MaxCachedExponent - MinCachedExponent + 1;
            var ret = new double[size];
            for (int exp = MinCachedExponent; exp <= MaxCachedExponent; exp++)
            {
                ret[exp - MinCachedExponent] = Math.Pow(10, exp);
            }
            return ret;
        }

        public static double GetPowerOfTen(int exponent)
        {
            _powerOfTenCache ??= InitCache();
            int index = exponent - MinCachedExponent;
            if ((uint)index < (uint)_powerOfTenCache.Length)
                return _powerOfTenCache[index];
            return Math.Pow(10, exponent);
        }

        public BigNumber(double mantissa, int exponent)
        {
            this.mantissa = mantissa;
            this.exponent = exponent;
        }
        public BigNumber(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Invalid number value");
            if (value == 0)
            {
                mantissa = 0;
                exponent = 0;
                return;
            }

            exponent = (int)Math.Floor(Math.Log10(Math.Abs(value)));
            mantissa = value / GetPowerOfTen(exponent);
        }        
        public BigNumber Normalize()
        {
            if (mantissa == 0) return Zero;

            double sign = Math.Sign(mantissa);
            double abs = Math.Abs(mantissa);

            double log10 = Math.Log10(abs);
            int shift = (int)Math.Floor(log10);

            return new BigNumber(
                sign * abs / Math.Pow(10, shift),
                exponent + shift
            );
        }
        public static bool TryCreate(double value, out BigNumber result)
        {
            try
            {
                result = new BigNumber(value);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        public static BigNumber operator +(BigNumber a, BigNumber b)
        {
            if (a.mantissa == 0) return b;
            if (b.mantissa == 0) return a;

            int diff = a.exponent - b.exponent;

            BigNumber result;
            if (diff >= 0)
            {
                result.exponent = a.exponent;
                result.mantissa = a.mantissa + b.mantissa / GetPowerOfTen(diff);
            }
            else
            {
                result.exponent = b.exponent;
                result.mantissa = a.mantissa / GetPowerOfTen(-diff) + b.mantissa;
            }

            return result.Normalize();
        }
        public static BigNumber operator +(BigNumber a, double b) => a + new BigNumber(b);
        public static BigNumber operator +(BigNumber a, float b) => a + new BigNumber(b);
        public static BigNumber operator +(BigNumber a, int b) => a + new BigNumber(b);
        public static BigNumber operator +(double a, BigNumber b) => new BigNumber(a) + b;
        public static BigNumber operator +(float a, BigNumber b) => new BigNumber(a) + b;
        public static BigNumber operator +(int a, BigNumber b) => new BigNumber(a) + b;
        
        public static BigNumber operator -(BigNumber a, BigNumber b) => a + (-b);
        public static BigNumber operator -(BigNumber a, double b) => a + new BigNumber(b * -1);
        public static BigNumber operator -(BigNumber a, float b) => a + new BigNumber(b * -1);
        public static BigNumber operator -(BigNumber a, int b) => a + new BigNumber(b * -1);
        public static BigNumber operator -(double a, BigNumber b) => new BigNumber(a) - b;
        public static BigNumber operator -(float a, BigNumber b) => new BigNumber(a) - b;
        public static BigNumber operator -(int a, BigNumber b) => new BigNumber(a) - b;
        
        public static BigNumber operator *(BigNumber a, BigNumber b)
        {
            BigNumber result;
            result.mantissa = a.mantissa * b.mantissa;
            result.exponent = a.exponent + b.exponent;
            return result.Normalize();
        }
        public static BigNumber operator *(BigNumber a, double b) => a * new BigNumber(b);
        public static BigNumber operator *(BigNumber a, float b) => a * new BigNumber(b);
        public static BigNumber operator *(BigNumber a, int b) => a * new BigNumber(b);
        public static BigNumber operator *(double a, BigNumber b) => new BigNumber(a) * b;
        public static BigNumber operator *(float a, BigNumber b) => new BigNumber(a) * b;
        public static BigNumber operator *(int a, BigNumber b) => new BigNumber(a) * b;
        
        public static BigNumber operator /(BigNumber a, BigNumber b)
        {
            if (b.mantissa == 0)
                throw new DivideByZeroException("Cannot divide by zero. BigNumber denominator is zero.");
    
            if (a.mantissa == 0)
                return new BigNumber(0);
    
            if (double.IsNaN(a.mantissa) || double.IsNaN(b.mantissa) ||
                double.IsInfinity(a.mantissa) || double.IsInfinity(b.mantissa))
            {
                throw new ArithmeticException("Operation contains invalid values (NaN/Infinity).");
            }

            double newMantissa = a.mantissa / b.mantissa;
            int newExponent = a.exponent - b.exponent;
    
            if (double.IsInfinity(newMantissa) || double.IsNaN(newMantissa))
            {
                throw new ArithmeticException("Division result is too large or undefined.");
            }

            BigNumber result = new BigNumber { mantissa = newMantissa, exponent = newExponent };
            return result.Normalize();
        }
        public static BigNumber operator /(BigNumber a, double b) => a / new BigNumber(b);
        public static BigNumber operator /(BigNumber a, float b) => a / new BigNumber(b);
        public static BigNumber operator /(BigNumber a, int b) => a / new BigNumber(b);
        public static BigNumber operator /(double a, BigNumber b) => new BigNumber(a) / b;
        public static BigNumber operator /(float a, BigNumber b) => new BigNumber(a) / b;
        public static BigNumber operator /(int a, BigNumber b) => new BigNumber(a) / b;
        
        public static bool operator ==(BigNumber a, BigNumber b)
        {
            return a.exponent == b.exponent && Math.Abs(a.mantissa - b.mantissa) < math.EPSILON;
        }
        public static bool operator !=(BigNumber a, BigNumber b) => !(a == b);

        public static bool operator <(BigNumber a, BigNumber b)
        {
            switch (a.mantissa)
            {
                case >= 0 when b.mantissa < 0:
                    return false;
                case < 0 when b.mantissa >= 0:
                    return true;
                case >= 0 when a.exponent != b.exponent:
                    return a.exponent < b.exponent;
                case >= 0:
                    return a.mantissa < b.mantissa;
            }

            return a.exponent != b.exponent 
                ? a.exponent > b.exponent 
                : a.mantissa > b.mantissa;
        }
        public static bool operator >(BigNumber a, BigNumber b) => b < a;

        public static bool operator <=(BigNumber a, BigNumber b) => a < b || a == b;
        public static bool operator >=(BigNumber a, BigNumber b) => a > b || a == b;
        
        public static BigNumber operator -(BigNumber a) => new() { mantissa = -a.mantissa, exponent = a.exponent };
        public static bool TryDivide(BigNumber a, BigNumber b, out BigNumber result)
        {
            try
            {
                result = a / b;
                return true;
            }
            catch (DivideByZeroException)
            {
                result = default;
                return false;
            }
        }
        
        public int CompareTo(BigNumber other)
        {
            if (mantissa == 0 && other.mantissa == 0) return 0;
    
            bool thisNonNegative = mantissa >= 0;
            bool otherNonNegative = other.mantissa >= 0;
    
            if (thisNonNegative != otherNonNegative)
                return thisNonNegative ? 1 : -1;
    
            if (exponent != other.exponent)
            {
                int expComparison = exponent.CompareTo(other.exponent);
                return thisNonNegative ? expComparison : -expComparison;
            }
    
            return mantissa.CompareTo(other.mantissa);
        }
        
        public bool Equals(BigNumber other) => mantissa.Equals(other.mantissa) && exponent == other.exponent;
        public override bool Equals(object obj) => obj is BigNumber other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(mantissa, exponent);
        
        public override string ToString()
        {
            return ToString(0);
        }        
        public string ToString(int precision)
        {
            if (mantissa == 0) return "0";
            return exponent switch
            {
                < 3 =>
                    $"{(mantissa * GetPowerOfTen(exponent)).ToString($"F{precision}", CultureInfo.InvariantCulture)}",
                >= 3 and < 6 =>
                    $"{(mantissa * GetPowerOfTen(exponent - 3)).ToString($"F{precision}", CultureInfo.InvariantCulture)}K",
                >= 6 and < 9 =>
                    $"{(mantissa * GetPowerOfTen(exponent - 6)).ToString($"F{precision}", CultureInfo.InvariantCulture)}M",
                >= 9 and < 12 =>
                    $"{(mantissa * GetPowerOfTen(exponent - 9)).ToString($"F{precision}", CultureInfo.InvariantCulture)}B",
                >= 12 and < 15 =>
                    $"{(mantissa * GetPowerOfTen(exponent - 12)).ToString($"F{precision}", CultureInfo.InvariantCulture)}T",
                _ => $"{mantissa.ToString($"F{precision}", CultureInfo.InvariantCulture)}e{exponent}"
            };
        }
        
        public static implicit operator BigNumber(float value)
        {
            return new BigNumber(value);
        }
        public static implicit operator BigNumber(double value)
        {
            return new BigNumber(value);
        }
        public static implicit operator BigNumber(int value)
        {
            return new BigNumber(value);
        }
        public static implicit operator float(BigNumber v)
        {
            // float.MaxValue ≈ 3.4e38
            if (v.exponent > 38)
                return v.mantissa >= 0 ? float.MaxValue : -float.MaxValue;

            if (v.exponent < -45)
                return 0f;

            return (float)(v.mantissa * Math.Pow(10, v.exponent));
        }
        public static implicit operator double(BigNumber v)
        {
            // float.MaxValue ≈ 3.4e38
            if (v.exponent > 38)
                return v.mantissa >= 0 ? double.MaxValue : -double.MaxValue;

            if (v.exponent < -45)
                return 0f;

            return (v.mantissa * Math.Pow(10, v.exponent));
        }
        
        public void Add(BigNumber variable)
        {
            this += variable;
        }
        public void Multiply(BigNumber variable)
        {
            this *= variable;
        }
        
        public static BigNumber Abs(BigNumber a) => new() { mantissa = Math.Abs(a.mantissa), exponent = a.exponent };
        public static BigNumber Sqrt(BigNumber a) => BigNumberExtensions.Pow(a, 0.5);
        public static BigNumber Log10(BigNumber a) => new BigNumber(Math.Log10(a.Mantissa) + a.Exponent);
    }
    
    public sealed class BigNumberStrictComparer : IEqualityComparer<BigNumber>
    {
        public bool Equals(BigNumber a, BigNumber b)
            => a.Exponent == b.Exponent
               && a.Mantissa.Equals(b.Mantissa);

        public int GetHashCode(BigNumber obj)
            => HashCode.Combine(obj.Mantissa, obj.Exponent);
    }
    public sealed class BigNumberNumericComparer : IEqualityComparer<BigNumber>
    {
        readonly double _relativeEpsilon;
        readonly int _exponentTolerance;

        public BigNumberNumericComparer(
            double relativeEpsilon = 1e-6,
            int exponentTolerance = 0)
        {
            _relativeEpsilon = relativeEpsilon;
            _exponentTolerance = exponentTolerance;
        }

        public bool Equals(BigNumber a, BigNumber b)
        {
            if (a.Mantissa == 0 && b.Mantissa == 0)
                return true;

            if (Math.Abs(a.Exponent - b.Exponent) > _exponentTolerance)
                return false;

            int exp = Math.Max(a.Exponent, b.Exponent);

            double ma = a.Mantissa * BigNumber.GetPowerOfTen(a.Exponent - exp);
            double mb = b.Mantissa * BigNumber.GetPowerOfTen(b.Exponent - exp);

            double diff = Math.Abs(ma - mb);
            double max = Math.Max(Math.Abs(ma), Math.Abs(mb));

            return diff <= max * _relativeEpsilon;
        }

        public int GetHashCode(BigNumber obj) => 0;
    }
    public sealed class BigNumberDisplayComparer : IEqualityComparer<BigNumber>
    {
        readonly int _precision;

        public BigNumberDisplayComparer(int precision)
            => _precision = precision;

        public bool Equals(BigNumber a, BigNumber b)
            => a.ToString(_precision) == b.ToString(_precision);

        public int GetHashCode(BigNumber obj) => 0;
    }
}