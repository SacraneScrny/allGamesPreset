using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using Sackrany.Actor.Traits.Storage.DataBase;
using Sackrany.Actor.Traits.Storage.Static;

using UnityEngine;

namespace Sackrany.Actor.Traits.Storage
{
    [Serializable] [JsonObject]
    public class Inventory : IDisposable, IEnumerable<Item>
    {
        [JsonProperty] [SerializeField] private List<Item> _items = new();
        [JsonIgnore] public IReadOnlyList<Item> Items => _items;

        [JsonProperty] [SerializeField] private uint _capacity;

        public Inventory(uint capacity = 0)
        {
            _capacity = capacity;
        }
        
        public Item this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }
        public Item this[Item item]
        {
            get
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    if (_items[i] == item)
                        return _items[i];
                }
                return default;
            }
            set
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    if (_items[i] == item)
                    {
                        _items[i] = value;
                        return;
                    }
                }
            }
        }
        
        public bool HasItem(Item item) => _items.Contains(item);
        public bool HasItem(int itemId) => _items.Any((x) => x.Id == itemId);

        public bool AddItem<T>(int count = 1) where T : ItemDefinition, new()
            => AddItem(ItemManager.GetItem<T>(), count);
        public bool AddItem(ItemDefinition def, int count = 1)
            => AddItem(new Item(def, count), count);
        public bool AddItem(Item item, int count = 1)
        {
            if (count == 0) return true;
            if (!IsAvailableToAddItem(item.Id, count)) return false;

            int remaining = count;

            for (int i = 0; i < _items.Count && remaining > 0; i++)
            {
                if (_items[i] != item)
                    continue;

                var slot = _items[i];
                if (slot.Count >= slot.MaxCount)
                    continue;

                int space = slot.MaxCount - slot.Count;
                int toAdd = remaining > space ? space : remaining;

                slot.Count += toAdd;
                _items[i] = slot;

                OnItemCountIncrease(slot, toAdd);
                ItemCountIncreased?.Invoke(this, slot, toAdd);
                remaining -= toAdd;
            }

            while (remaining > 0)
            {
                if (_capacity > 0 && _items.Count >= _capacity)
                    return false;

                int toAdd = remaining > item.MaxCount ? item.MaxCount : remaining;

                var newItem = new Item(ItemManager.GetData(item), toAdd);
                _items.Add(newItem);

                OnItemAdded(newItem);
                ItemAdded?.Invoke(this, newItem, toAdd);
                remaining -= toAdd;
            }

            return true;
        }
        public bool AddItem(int itemId, int count = 1)
        {
            return AddItem(ItemManager.GetData(itemId), count);
        }

        public bool RemoveItem(Item item, int count = 1) => RemoveItem(item.Id, count);
        public bool RemoveItem(int itemId, int count = 1)
        {
            if (count == 0) return true;

            int remaining = count;

            for (int i = _items.Count - 1; i >= 0 && remaining > 0; i--)
            {
                if (_items[i].Id != itemId)
                    continue;

                var slot = _items[i];

                if (slot.Count > remaining)
                {
                    slot.Count -= remaining;
                    _items[i] = slot;

                    OnItemCountDecrease(slot, remaining);
                    ItemCountDecreased?.Invoke(this, slot, remaining);
                    remaining = 0;
                }
                else
                {
                    remaining -= slot.Count;
                    OnItemCountDecrease(slot, slot.Count);
                    ItemCountDecreased?.Invoke(this, slot, remaining);

                    _items.RemoveAt(i);
                    ItemRemoved?.Invoke(this, slot);
                    OnItemRemoved(slot);
                }
            }

            return remaining == 0;
        }
        public bool RemoveAll(int itemId)
        {
            bool removed = false;

            for (int i = _items.Count - 1; i >= 0; i--)
            {
                if (_items[i].Id != itemId)
                    continue;

                var item = _items[i];
                _items.RemoveAt(i);
                OnItemRemoved(item);
                ItemRemoved?.Invoke(this, item);
                removed = true;
            }

            return removed;
        }

        protected virtual bool IsAvailableToAddItem(int itemId, int count) => true;

        protected virtual void OnItemAdded(Item item) { }
        protected virtual void OnItemRemoved(Item item) { }

        protected virtual void OnItemCountIncrease(Item item, int count) { }
        protected virtual void OnItemCountDecrease(Item item, int count) { }
        
        [field: NonSerialized] public event Action<Inventory, Item, int> ItemAdded;
        [field: NonSerialized] public event Action<Inventory, Item> ItemRemoved;
        [field: NonSerialized] public event Action<Inventory, Item, int> ItemCountIncreased;
        [field: NonSerialized] public event Action<Inventory, Item, int> ItemCountDecreased;
        
        public IEnumerator<Item> GetEnumerator()
        {
            return _items.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public void Dispose()
        {
            ItemAdded = null;
            ItemRemoved = null;
            ItemCountIncreased = null;
            ItemCountDecreased = null;
        }
    }
}