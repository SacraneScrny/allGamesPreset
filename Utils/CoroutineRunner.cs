using UnityEngine;

namespace Sackrany.Utils
{
    public class CoroutineRunner : MonoBehaviour
    {
        public static CoroutineRunner Instance
        {
            get
            {
                GameObject go = new GameObject("CoroutineRunner");
                return go.AddComponent<CoroutineRunner>();
            }
        }
    }
}