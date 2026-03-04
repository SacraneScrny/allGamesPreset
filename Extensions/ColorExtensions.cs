using UnityEngine;

namespace Sackrany.Extensions
{
    public static class ColorExtensions
    {
        public static Color SetAlpha(this Color color, float alpha) 
            => new(color.r, color.g, color.b, alpha);
        
        public static Color With(
            this Color color,
            float? r = null,
            float? g = null,
            float? b = null,
            float? a = null)
            => new Color(
                r ?? color.r,
                g ?? color.g,
                b ?? color.b,
                a ?? color.a
            );

        public static Color Add(
            this Color color,
            float? r = null,
            float? g = null,
            float? b = null,
            float? a = null)
            => new Color(
                color.r + (r ?? 0f),
                color.g + (g ?? 0f),
                color.b + (b ?? 0f),
                color.a + (a ?? 0f)
            );

        public static Color Multiply(
            this Color color,
            float? r = null,
            float? g = null,
            float? b = null,
            float? a = null)
            => new Color(
                color.r * (r ?? 1f),
                color.g * (g ?? 1f),
                color.b * (b ?? 1f),
                color.a * (a ?? 1f)
            );
    }
}