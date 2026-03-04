using System;

using Sackrany.Actor.Modules;
using Sackrany.Actor.Modules.ModuleComposition;
using Sackrany.Actor.Modules.Modules;
using Sackrany.Actor.Traits.Storage;
using Sackrany.Actor.Traits.Storage.DataBase;
using Sackrany.Actor.Traits.Storage.Static;

using UnityEngine;

namespace Sackrany.Actor.DefaultFeatures.StorageFeature
{
    public class StorageModule : Module, ISerializableModule
    {
        public Inventory Inventory;
        [Dependency] IUseItemModule[] _useItemModules;
        [Template] Storage _storage;
        
        protected override void OnStart()
        {
            Inventory = new Inventory((uint)_storage.capacity);
        }

        public void Use(Item item)
        {
            if (!Inventory.HasItem(item)) return;
            item = Inventory[item];
            
            if (!item.IsUsed)
            {
                ForceUse(item);
                item.IsUsed = true;
            }
            else
            {
                ForceUnUse(item);
                item.IsUsed = false;
            }

            Inventory[item] = item;
        }
        public void AddItem<T>(int count = 1, bool autoUse = false) where T : ItemDefinition, new()
            => AddItem(ItemManager.GetItem<T>(), count, autoUse);
        public void AddItem(Item item, int count = 1, bool autoUse = false)
        {
            if (Inventory.AddItem(item, count) && autoUse)
                Use(item);
        }
        public void RemoveItem(Item item, int count = 1)
        {
            if (!Inventory.HasItem(item)) return;
            if (Inventory[item].IsUsed)
                ForceUnUse(item);
            Inventory.RemoveItem(item, count);
        }
        
        public void ForceUse(Item item)
        {
            foreach (var module in _useItemModules)
                module.Use(item);
            Debug.Log($"Using item: {item}");
        }
        public void ForceUnUse(Item item)
        {
            foreach (var module in _useItemModules)
                module.UnUse(item);
        }
        
        public object[] Serialize() => new object[] { Inventory };
        public void Deserialize(object[] data)
        {
            Inventory = (Inventory)data[0];
            
            foreach (var item in Inventory)
                if (item.IsUsed)
                    ForceUse(item);
        }
    }

    [Serializable]
    public struct Storage : ModuleTemplate<StorageModule>
    {
        public int capacity;
    } 
    public interface IUseItemModule
    {
        public void Use(Item item);
        public void UnUse(Item item);
    }
}