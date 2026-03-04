using System.Collections.Generic;

using UnityEngine;

namespace Sackrany.Actor.Traits.Storage.DataBase
{
    [CreateAssetMenu(fileName = "ItemsDatabase", menuName = "Create ItemsDatabase")]
    public class ItemsDatabase : ScriptableObject
    {
        [SerializeField][SerializeReference][SubclassSelector]
        public List<ItemDefinition> Items = new();
    }
}