using System.Collections.Generic;

using Sackrany.Actor.Traits.Storage.DataBase;

using UnityEngine;

namespace Sackrany.Actor.Traits.Storage.Static
{
    public static class ItemManager
    {
        static readonly Dictionary<int, ItemDefinition> _items = new();
        static readonly Dictionary<ItemConfig, ItemDefinition> _configToDef = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            var db = Resources.Load<ItemsDatabase>("ItemsDatabase");
            if (db == null) return;
            foreach (var def in db.Items)
                Register(def);
        }

        public static void Register(ItemDefinition def)
        {
            _items.TryAdd(def.Id, def);
            if (def.Config != null) _configToDef.TryAdd(def.Config, def);
        }
        public static ItemDefinition GetData(int id) => _items.GetValueOrDefault(id);
        public static ItemDefinition GetData(Item item) => GetData(item.Id);
        public static ItemDefinition GetDataByConfig(ItemConfig config)
            => config != null ? _configToDef.GetValueOrDefault(config) : null;

        public static Item GetItem<T>() where T : ItemDefinition, new()
        {
            var def = GetOrCreate<T>();
            return new Item(def);
        }

        static ItemDefinition GetOrCreate<T>() where T : ItemDefinition, new()
        {
            var tmp = new T();
            if (_items.TryGetValue(tmp.Id, out var def)) return def;
            Register(tmp);
            return tmp;
        }
    }
}