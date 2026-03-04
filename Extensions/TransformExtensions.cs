using System;
using System.Collections.Generic;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Sackrany.Extensions
{
    public static class TransformExtensions
    {
        public static IEnumerable<Transform> Children(this Transform transform)
        {
            foreach (Transform child in transform)
                yield return child;
        }

        public static void DestroyChildren(this Transform transform)
            => transform.PerformActionOnChildren((c) => Object.Destroy(c.gameObject));

        public static void EnableChildren(this Transform transform)
            => transform.PerformActionOnChildren((c) => c.gameObject.SetActive(true));

        public static void DisableChildren(this Transform transform)
            => transform.PerformActionOnChildren((c) => c.gameObject.SetActive(false));

        public static void PerformActionOnChildren(this Transform transform, Action<Transform> action)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                action(transform.GetChild(i));
        }
    }
}