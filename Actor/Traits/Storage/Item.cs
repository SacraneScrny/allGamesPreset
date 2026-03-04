using System;

using Newtonsoft.Json;

using Sackrany.Actor.Traits.Storage.DataBase;
using Sackrany.Actor.Traits.Storage.Static;
using Sackrany.Utils.Hash;

namespace Sackrany.Actor.Traits.Storage
{
    [Serializable]
    public struct Item : IEquatable<Item>
    {
        [JsonProperty] readonly int _typeId;
        [JsonProperty] readonly uint _instanceId;

        [JsonProperty] public int Count;
        [JsonProperty] public bool IsUsed;

        [JsonIgnore] public int Id => _typeId;
        [JsonIgnore] public int MaxCount => ItemManager.GetData(_typeId)?.MaxCount ?? 1;
        [JsonIgnore] public bool IsStackable => ItemManager.GetData(_typeId)?.IsStackable ?? true;
        [JsonIgnore] public bool IsUniqueContext => ItemManager.GetData(_typeId)?.IsUniqueContext ?? false;

        public Item(ItemDefinition def, int count = 1)
        {
            _typeId = def.Id;
            _instanceId = SimpleId.Next();
            Count = count;
            IsUsed = false;
        }

        public bool Equals(Item other)
            => IsStackable
                ? _typeId == other._typeId
                : _instanceId == other._instanceId;

        public override bool Equals(object obj) => obj is Item other && Equals(other);

        public override int GetHashCode()
            => IsStackable
                ? _typeId.GetHashCode()
                : _instanceId.GetHashCode();

        public static bool operator ==(Item l, Item r) => l.Equals(r);
        public static bool operator !=(Item l, Item r) => !(l == r);
    }
}