using System;
using System.Collections.Generic;

namespace Sackrany.Extensions
{
    public static class ListExtensions
    {
        public static bool FastRemove<T>(this List<T> list, T item)
            where T : class
        {
            int index = list.IndexOf(item);
            if (index < 0) return false;
    
            int lastIndex = list.Count - 1;
            if (index != lastIndex)
            {
                list[index] = list[lastIndex];
            }
    
            list.RemoveAt(lastIndex); 
            return true;
        }
        public static bool FastRemove<T>(this T[] array, T item)
            where T : class
        {
            int index = Array.IndexOf(array, item);
            if (index < 0) return false;
    
            int lastIndex = array.Length - 1;
            if (index != lastIndex)
            {
                array[index] = array[lastIndex];
            }
            Array.Resize(ref array, array.Length - 1);
            return true;
        }
    }
}