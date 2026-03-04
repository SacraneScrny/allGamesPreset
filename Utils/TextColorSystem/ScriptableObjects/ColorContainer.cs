using System;
using System.Collections.Generic;
using System.Linq;

using Sackrany.Utils.Hash;

using UnityEngine;

namespace Sackrany.Utils.TextColorSystem.ScriptableObjects
{
    [CreateAssetMenu(fileName = "DefaultColorContainer", menuName = "Create/Color Container")]
    public class ColorContainer : ScriptableObject
    {
        private Dictionary<uint, ColorContainerElement> colorContainerDictionary = new ();
        public List<ColorContainerElement> Elements = new();

        public Color GetColorByTag(string tag)
        {
            colorContainerDictionary ??= Elements.ToDictionary(e => e.Tag.XXHash());
            return colorContainerDictionary.TryGetValue(tag.XXHash(), out var val) ? val.Color : Color.white;
        }

        public string GetHexColorByTag(string tag)
        {
            var color = GetColorByTag(tag);

            var r = (byte)(color.r * 255);
            var g = (byte)(color.g * 255);
            var b = (byte)(color.b * 255);
            var a = (byte)(color.a * 255);

            var hex = $"{r:X2}{g:X2}{b:X2}{a:X2}".ToUpper();

            return hex;
        }
    }

    [Serializable]
    public class ColorContainerElement
    {
        public string Tag;
        public Color Color;
    }
}