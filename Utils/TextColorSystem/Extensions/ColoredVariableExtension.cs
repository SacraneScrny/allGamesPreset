using System.Globalization;

using Sackrany.Utils.TextColorSystem.Singletones;

namespace Sackrany.Utils.TextColorSystem.Extensions
{
    public static class ColoredVariableExtension
    {
        public static string Colored<T>(this T current_object, string color_tag)
        {
            return TextColorSingletone.GetColorBlock(color_tag) + current_object.ToString() + TextColorSingletone.GetColorEndBlock();
        }
        public static string Colored<T>(this T current_object, TextType text_type)
        {
            if (text_type == TextType.Default) return current_object.ToString();
            return Colored<T>(current_object, text_type.ToString());
        }
        public static string Colored(this float number, string color_tag, int decimalNum = 0, string additionalSymbols = "")
        {
            return Colored<string>(FloatToStr(number, decimalNum, additionalSymbols), color_tag);
        }
        public static string Colored(this float number, TextType text_type, int decimalNum = 0, string additionalSymbols = "")
        {
            if (text_type == TextType.Default) return number.ToString(CultureInfo.InvariantCulture);
            return Colored(number, text_type.ToString(), decimalNum, additionalSymbols);
        }

        /// <summary>
        /// it's not a block
        /// </summary>
        /// <param name="number"></param>
        /// <param name="decimalNum"></param>
        /// <param name="additionalSymbols"></param>
        /// <returns></returns>
        public static string FloatToStr(this float number, int decimalNum, string additionalSymbols = "")
        {
            return string.Format("{0}{1}", number.ToString($"F{decimalNum}"), additionalSymbols);
        }

        public static string Sprite<T>(this T current_object, string sprite_tag, bool toLeft = true)
        {
            if (toLeft)
                return TextColorSingletone.GetSpriteBlock(sprite_tag) + current_object.ToString();
            return current_object.ToString() + TextColorSingletone.GetSpriteBlock(sprite_tag);
        }
        public static string Sprite(this string original_string, string sprite_tag, bool toLeft = true)
        {
            return Sprite<string>(original_string, sprite_tag, toLeft);
        }
        public static string Sprite(this string original_string, TextType sprite_type, bool toLeft = true)
        {
            if (sprite_type == TextType.Default) return original_string;
            return Sprite(original_string, sprite_type.ToString(), toLeft);
        }

        public static string Bold(this string original_string)
        {
            return TextColorSingletone.GetCustomBlock("b") + original_string + TextColorSingletone.GetCustomEndBlock("b");
        }
        public static string Italic(this string original_string)
        {
            return TextColorSingletone.GetCustomBlock("i") + original_string + TextColorSingletone.GetCustomEndBlock("i");
        }

        /// <summary>
        /// works only as first tag
        /// for colored numbers, use FloatToStr then Opacity and then Colored
        /// </summary>
        /// <param name="original_string"></param>
        /// <param name="opacity"></param>
        /// <returns></returns>
        public static string Opacity(this string original_string, float opacity)
        {
            return TextColorSingletone.GetAlphaBlock(opacity) + original_string;
        }
        public static string Opacity(this float number, float opacity, int decimalNum, string additionalSymbols = "")
        {
            return Opacity(FloatToStr(number, decimalNum, additionalSymbols), opacity);
        }
    }

    public enum TextType
    {
        none = -1,
        Default = 0,
        blood,
        blush,
        cloud,
        condom,
        crystal,
        cum,
        dildo,
        fire,
        food,
        handcuffs,
        heart,
        lactation,
        lightning,
        @lock,
        paper,
        pigeon,
        question,
        skull,
        soap,
        sphere,
        stink,
        sword,
        time,
        warning,
        wind,
        level,
        shield,
        shield_broke,
        Common,
        Uncommon,
        Rare,
        ExtraRare,
        crit,
        dmgDown,
        dmgUp,
        patreon,
        radius,
        speedDown,
        speedUp,
        meat,
        eye,
        souls,
        rayder
    }
}
