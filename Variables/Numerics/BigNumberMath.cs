using System;

namespace Sackrany.Variables.Numerics
{
    public static class BigNumberMath
    {
        // ---------- Pow ----------

        public static BigNumber Pow(BigNumber baseValue, double exponentValue)
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

            if (product > int.MaxValue || product < int.MinValue)
                throw new OverflowException($"Exponent result {product} exceeds int range");

            int k = (int)Math.Floor(product);
            double f = product - k;

            double tenToF = Math.Pow(10, f);
            double mPow = Math.Pow(m, exponentValue);

            double newMantissa = mPow * tenToF;
            int newExponent = k;

            return new BigNumber(newMantissa, newExponent).Normalize();
        }

        public static BigNumber Pow(double baseValue, double exponentValue)
        {
            return Pow(new BigNumber(baseValue), exponentValue);
        }

        // ---------- Lerp ----------

        public static BigNumber Lerp(BigNumber a, BigNumber b, float t)
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

        // ---------- Scale ----------

        public static BigNumber Scale(BigNumber value, float factor)
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

        // ---------- Min / Max ----------

        public static BigNumber Min(BigNumber a, BigNumber b)
        {
            return a.CompareTo(b) <= 0 ? a : b;
        }

        public static BigNumber Max(BigNumber a, BigNumber b)
        {
            return a.CompareTo(b) >= 0 ? a : b;
        }
    }
}