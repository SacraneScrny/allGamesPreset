using System;
using System.Collections;

using UnityEngine;

namespace Sackrany.Utils
{
    public class DeferredExecution : AManager<DeferredExecution>
    {
        public static void Execute(GameObject @object, Func<GameObject, bool> condition)
        {
            if (condition(@object))
            {
                @object.SetActive(!@object.activeSelf);
                return;
            }
            Instance.StartCoroutine(deferredRoutine(@object, condition, (x) => x.SetActive(!x.activeSelf)));
        }
        public static void Execute<T>(T @object, Func<T, bool> condition, Action<T> onCond)
        {
            if (condition(@object))
            {
                onCond(@object);
                return;
            }
            Instance.StartCoroutine(deferredRoutine(@object, condition, onCond));
        }

        private static IEnumerator deferredRoutine<T>(T @object, Func<T, bool> condition, Action<T> onCond)
        {
            while (!condition(@object)) yield return null;
            onCond(@object);
        }
    }
}