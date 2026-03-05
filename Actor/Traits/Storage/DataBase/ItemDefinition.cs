using System;

using Sackrany.Actor.Traits.Storage.DataBase.Behaviour;
using Sackrany.Actor.UnitMono;

namespace Sackrany.Actor.Traits.Storage.DataBase
{
    [Serializable]
    public abstract class ItemDefinition
    {    
        public abstract ItemConfig Config { get; }

        public int Id => Config.Hash;
        public int MaxCount => Config.MaxCount;
        public bool IsStackable => Config.IsStackable;
        public bool IsUniqueContext => Config.IsUniqueContext;
        public Item GetItem() => new Item(this);

        public virtual void OnUse(Unit unit, ItemUseContext ctx)
        {
            foreach (var b in Config.OnUseBehaviours) b.OnUse(unit, ctx);
        }
        public virtual void OnUnUse(Unit unit, ItemUseContext ctx)
        {
            foreach (var b in Config.OnUnUseBehaviours) b.OnUnUse(unit, ctx);
        }
    }
}