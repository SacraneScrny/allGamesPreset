using System.Linq;

using Sackrany.Utils.TextColorSystem.ScriptableObjects;

using UnityEngine;

namespace Sackrany.Utils.TextColorSystem.Singletones
{
    public static class TextColorSingletone
    {
        private static ColorContainer _currentColorContainer;

        public static ColorContainer Container
        {
            get
            {
                _currentColorContainer ??= Resources.LoadAll<ColorContainer>("").First();
                return _currentColorContainer;
            }
        }

        public static string GetColorBlock(string color_tag)
        {
            return "<color=#" + Container.GetHexColorByTag(color_tag) + ">";
        }

        public static string GetColorEndBlock()
        {
            return "</color>";
        }

        public static string GetSpriteBlock(string sprite_tag)
        {
            return "<sprite name=\"" + sprite_tag + "\">";
        }

        public static string GetAlphaBlock(float alpha)
        {
            var a = (byte)(alpha * 255);
            return string.Format("<alpha=#{0:X2}>", a);
        }

        public static string GetCustomBlock(string block)
        {
            return string.Format("<{0}>", block);
        }

        public static string GetCustomEndBlock(string block)
        {
            return string.Format("</{0}>", block);
        }
    }
}