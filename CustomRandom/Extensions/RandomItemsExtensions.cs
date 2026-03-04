using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using Sackrany.CustomRandom.Entities;
using Sackrany.CustomRandom.Global;
using Sackrany.CustomRandom.Interfaces;

using Unity.Mathematics;

using UnityEngine;

namespace Sackrany.CustomRandom.Extensions
{
    public static class RandomItemsExtensions
    {
        public static RandomItem[] RemoveById(this RandomItem[] items, params int[] ids)
        {
            var ret = new List<RandomItem>();
            foreach (var item in items.Where(x => !ids.Contains(x.id)))
                ret.Add(item);
            return ret.ToArray();
        }
        public static List<RandomItem> RemoveById(this List<RandomItem> items, params int[] ids)
        {
            var ret = new List<RandomItem>();
            foreach (var item in items.Where(x => !ids.Contains(x.id)))
                ret.Add(item);
            return ret;
        }
        
        public static int GetRandomItem(this RandomItem[] array, IRandom rnd, int significantDecimal = 5)
        {
            if (array.Length == 0)
                return 0;
            rnd ??= GlobalRandom.Current;

            var chanceMultiply = Mathf.Pow(10, significantDecimal);
            var totalWeight = 0;

            foreach (var a in array)
                totalWeight += Mathf.RoundToInt(a.chance * chanceMultiply);

            var rndNumber = rnd.Next(totalWeight);

            var cumulativeWeight = 0;
            foreach (var a in array)
            {
                cumulativeWeight += Mathf.RoundToInt(a.chance * chanceMultiply);
                if (rndNumber < cumulativeWeight)
                    return a.id;
            }

            return 0;
        }

        public static int GetRandomItem(this List<RandomItem> list, IRandom rnd, int significantDecimal = 5)
        {
            return GetRandomItem(list.ToArray(), rnd, significantDecimal);
        }
        public static int GetRandomItem<T>(this T[] genericArray, float[] chances, int significantDecimal = 5)
        {
            return GetRandomItem(GenerateRandomItemsArray(genericArray, chances), GlobalRandom.Current, significantDecimal);
        }
        public static int GetRandomItem<T>(this T[] genericArray, float[] chances, IRandom rnd, int significantDecimal = 5)
        {
            return GetRandomItem(GenerateRandomItemsArray(genericArray, chances), rnd, significantDecimal);
        }
        public static int GetRandomItem<T>(this T[] genericArray, int[] chances, IRandom rnd, int significantDecimal = 5)
        {
            return GetRandomItem(GenerateRandomItemsArray(genericArray, chances), rnd, significantDecimal);
        }
        public static int GetRandomItem<T>(this List<T> genericList, float[] chances, IRandom rnd, int significantDecimal = 5)
        {
            return GetRandomItem(GenerateRandomItemsArray(genericList.ToArray(), chances), rnd, significantDecimal);
        }
        public static int GetRandomItem<T>(this List<T> genericList, int[] chances, IRandom rnd, int significantDecimal = 5)
        {
            return GetRandomItem(GenerateRandomItemsArray(genericList.ToArray(), chances), rnd, significantDecimal);
        }

        public static RandomItem[] GenerateRandomItemsArray<T>(this T[] genericArray, float[] chances = null)
        {
            if (chances == null || chances.Length != genericArray.Length)
                chances = Enumerable.Repeat(1f, genericArray.Length).ToArray();
            var ret = new RandomItem[genericArray.Length];
            for (var i = 0; i < genericArray.Length; i++)
                ret[i] = new RandomItem(i, chances[i]);
            return ret;
        }
        public static RandomItem[] GenerateRandomItemsArray<T>(this T[] genericArray, int[] chances = null)
        {
            if (chances == null || chances.Length == 0)
                chances = Enumerable.Repeat(100, genericArray.Length).ToArray();
            float[] fChances = new float[chances.Length];
            for (var i = 0; i < chances.Length; i++)
                fChances[i] = chances[i] / 100f;
            return GenerateRandomItemsArray(genericArray, fChances);
        }
        public static RandomItem[] GenerateRandomItemsArray<T>(this List<T> genericArray, float[] chances = null)
        {
            if (chances == null || chances.Length != genericArray.Count)
                chances = Enumerable.Repeat(1f, genericArray.Count).ToArray();
            var ret = new RandomItem[genericArray.Count];
            for (var i = 0; i < genericArray.Count; i++)
                ret[i] = new RandomItem(i, chances[i]);
            return ret;
        }
        public static RandomItem[] GenerateRandomItemsArray<T>(this List<T> genericArray, int[] chances = null)
        {
            if (chances == null || chances.Length != genericArray.Count)
                chances = Enumerable.Repeat(100, genericArray.Count).ToArray();
            float[] fChances = new float[chances.Length];
            for (var i = 0; i < chances.Length; i++)
                fChances[i] = chances[i] / 100f;
            return GenerateRandomItemsArray(genericArray, fChances);
        }

        public static T[] Shuffle<T>(this T[] array, float[] chances = null)
            => Shuffle(array, GlobalRandom.Current, chances);
        public static T[] Shuffle<T>(this T[] array, ref IRandom rnd, float[] chances = null)
            => Shuffle(array, ref rnd, chances);
        public static T[] Shuffle<T>(this T[] array, IRandom rnd, float[] chances = null)
        {
            if (chances == null || chances.Length != array.Length)
                chances = Enumerable.Repeat(1f, array.Length).ToArray();
            if (array.Length <= 1)
                return array;
            var weightedKeys = new KeyValuePair<float, T>[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                float key = -math.log(rnd.NextFloat()) / chances[i];

                weightedKeys[i] = new KeyValuePair<float, T>(key, array[i]);
            }
            
            Array.Sort(weightedKeys, (a, b) => a.Key.CompareTo(b.Key));

            var shuffledArray = new T[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                shuffledArray[i] = weightedKeys[i].Value;
            }

            return shuffledArray;
        }
        
        public static int[] GetRandomPositions<T>(this T[] genericArray, int count, ref IRandom rnd)
        {
            if (count > genericArray.Length)
                return Array.Empty<int>();
            rnd ??= GlobalRandom.Current;
            var ret = new int[count];
            var nums = genericArray.Select((x, y) => y).ToList();
            for (var i = 0; i < count; i++)
            {
                var localPos = GlobalRandom.Current.Next(nums.Count);
                ret[i] = nums[localPos];
                nums.RemoveAt(localPos);
            }

            return ret;
        }
        public static int[] GetRandomPositions<T>(this List<T> genericList, int count, ref IRandom rnd)
        {
            return GetRandomPositions(genericList.ToArray(), count, ref rnd);
        }
        public static int[] GetRandomPositions<T>(this T[] genericArray, ref IRandom rnd)
        {
            rnd ??= GlobalRandom.Current;
            return GetRandomPositions(genericArray, rnd.Next(1, genericArray.Length), ref rnd);
        }
        public static int[] GetRandomPositions<T>(this List<T> genericList, ref IRandom rnd)
        {
            rnd ??= GlobalRandom.Current;
            return GetRandomPositions(genericList.ToArray(), rnd.Next(1, genericList.Count), ref rnd);
        }
    }
}