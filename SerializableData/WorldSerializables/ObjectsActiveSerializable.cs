using System.Linq;

using Sackrany.SerializableData.Components;

using UnityEngine;

namespace Sackrany.SerializableData.WorldSerializables
{
    public class ObjectsActiveSerializable : SerializableBehaviour
    {
        public GameObject[] SerializableObjects;
        
        private protected override void OnRegister()
        {
            RegisterSerializable<bool[]>(
                "objects::active",
                () => SerializableObjects.Select(x => x.activeSelf).ToArray(),
                (objs) =>
                {
                    for (int i = 0; i < objs.Length; i++)
                        SerializableObjects[i].SetActive(objs[i]);
                });
        }
    }
}