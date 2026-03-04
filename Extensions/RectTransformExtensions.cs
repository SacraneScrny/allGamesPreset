using UnityEngine;
using UnityEngine.UI;

namespace Sackrany.Extensions
{
    public static class RectTransformExtensions
    {
        public static void RebuildLayout(this RectTransform rectTransform)
        {
            foreach (var a in rectTransform.GetComponentsInChildren<RectTransform>())
                LayoutRebuilder.ForceRebuildLayoutImmediate(a);
            foreach (var a in rectTransform.GetComponentsInChildren<RectTransform>())
                LayoutRebuilder.ForceRebuildLayoutImmediate(a);

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform.GetComponent<RectTransform>());
        }
    }
}