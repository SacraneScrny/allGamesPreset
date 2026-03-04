using System;

using Sackrany.Actor.Traits.Storage.DataBase.Behaviour;
using Sackrany.Utils.Hash;

using UnityEngine;

namespace Sackrany.Actor.Traits.Storage.DataBase
{
    [CreateAssetMenu(fileName = "ItemConfig", menuName = "Items/Config")]
    public class ItemConfig : ScriptableObject
    {
        Guid _itemGuid;
        int _hash;
        public Guid ItemGuid => _itemGuid;
        public int Hash => _hash;
        void OnValidate()
        {
            if (_itemGuid == Guid.Empty)
            {
                _itemGuid = Guid.NewGuid();
                _hash = _itemGuid.ToString().Hash();
            }
        }
        
        public string DisplayName;
        public int MaxCount = 99;
        public bool IsStackable = true;
        public bool IsUniqueContext = false;
        public Sprite Icon;

        [SerializeField][SerializeReference][SubclassSelector]
        public IItemBehaviour[] OnUseBehaviours = Array.Empty<IItemBehaviour>();

        [SerializeField][SerializeReference][SubclassSelector]
        public IItemBehaviour[] OnUnUseBehaviours = Array.Empty<IItemBehaviour>();
    }
}