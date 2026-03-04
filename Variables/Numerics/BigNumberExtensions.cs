using System;
using System.Globalization;

using Unity.Mathematics;

namespace Sackrany.Variables.Numerics
{
    public static class BigNumberExtensions
    {
        private static readonly string[] BaseSuffixes =
        {
            "", "K", "M", "B", "T"
        };

        private static readonly double[] Pow10 =
        {
            1.0, 10.0, 100.0
        };

        public static string ToAlphabetStringFast(this BigNumber n, int precision = 2)
        {
            if (n.Mantissa == 0)
                return "0";
            precision = n.Exponent > 1 ? 0 : math.min(precision, 9);

            int tier = n.Exponent / 3;
            int rem  = n.Exponent - tier * 3; // 0..2

            double value = n.Mantissa * Pow10[rem];

            if (value >= 1000.0)
            {
                value /= 1000.0;
                tier++;
            }

            Span<char> buffer = stackalloc char[64];
            int pos = 0;

            Span<char> format = stackalloc char[2];
            format[0] = 'F';
            format[1] = (char)('0' + precision);

            value.TryFormat(
                buffer,
                out int written,
                format,
                CultureInfo.InvariantCulture);

            pos += written;

            if (tier < BaseSuffixes.Length)
            {
                BaseSuffixes[tier].AsSpan().CopyTo(buffer[pos..]);
                pos += BaseSuffixes[tier].Length;
            }
            else
            {
                pos += WriteAlphabetSuffix(buffer[pos..], tier - BaseSuffixes.Length);
            }

            return new string(buffer[..pos]);
        }

        private static int WriteAlphabetSuffix(Span<char> dst, int index)
        {
            // 0 -> a
            // 25 -> z
            // 26 -> aa
            index += 1;

            int len = 0;
            Span<char> tmp = stackalloc char[8]; // хватит до zzzzzz

            while (index > 0)
            {
                index--;
                tmp[len++] = (char)('a' + (index % 26));
                index /= 26;
            }

            for (int i = 0; i < len; i++)
                dst[i] = tmp[len - 1 - i];

            return len;
        }
        
        public static BigNumber Pow(this BigNumber baseValue, double exponentValue)
        {
            switch (baseValue.Mantissa)
            {
                case 0 when exponentValue == 0:
                    throw new ArgumentException("0^0 is undefined");
                case 0 when exponentValue < 0:
                    throw new DivideByZeroException("Cannot raise zero to a negative power");
                case 0:
                    return BigNumber.Zero;
                case < 0 when exponentValue % 1 != 0:
                    throw new ArgumentException("Cannot raise a negative number to a fractional power");
            }

            double m = baseValue.Mantissa;
            int e = baseValue.Exponent;

            double product = e * exponentValue;
            const double maxInt = int.MaxValue;
            const double minInt = int.MinValue;
    
            if (product > maxInt || product < minInt)
                throw new OverflowException($"Exponent result {product} exceeds int range");

            int k = (int)Math.Floor(product);
            double f = product - k;

            double tenToF = Math.Pow(10, f);
            double mPow = Math.Pow(m, exponentValue);
            double newMantissa = mPow * tenToF;
            int newExponent = k;

            BigNumber result = new BigNumber(newMantissa, newExponent);
            return result.Normalize();
        }
        public static BigNumber Pow(double baseValue, double exponentValue) 
            => Pow(new BigNumber(baseValue), exponentValue);
        
        public static BigNumber Lerp(this BigNumber a, BigNumber b, float t)
        {
            if (t <= 0f) return a;
            if (t >= 1f) return b;

            if (a.Mantissa == 0) return Scale(b, t);
            if (b.Mantissa == 0) return Scale(a, 1f - t);

            int targetExp = Math.Max(a.Exponent, b.Exponent);

            double ma = a.Mantissa * BigNumber.GetPowerOfTen(a.Exponent - targetExp);
            double mb = b.Mantissa * BigNumber.GetPowerOfTen(b.Exponent - targetExp);

            double m = ma + (mb - ma) * t;

            if (m == 0)
                return BigNumber.Zero;

            double abs = Math.Abs(m);
            int shift = (int)Math.Floor(Math.Log10(abs));

            return new BigNumber(
                m / BigNumber.GetPowerOfTen(shift),
                targetExp + shift
            );
        }
        public static BigNumber Scale(this BigNumber value, float factor)
        {
            if (value.Mantissa == 0 || factor == 0f)
                return BigNumber.Zero;

            double m = value.Mantissa * factor;
            double abs = Math.Abs(m);

            int shift = (int)Math.Floor(Math.Log10(abs));

            return new BigNumber(
                m / BigNumber.GetPowerOfTen(shift),
                value.Exponent + shift
            );
        }
    }
}