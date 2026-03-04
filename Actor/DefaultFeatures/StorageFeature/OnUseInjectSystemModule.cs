using System;
using System.Collections.Generic;

using Sackrany.Actor.Modules.Modules;
using Sackrany.Actor.Traits.Storage;
using Sackrany.Actor.Traits.Storage.DataBase;
using Sackrany.Actor.Traits.Storage.DataBase.Behaviour;
using Sackrany.Actor.Traits.Storage.Static;

namespace Sackrany.Actor.DefaultFeatures.StorageFeature
{
    public class OnUseInjectSystemModule : Module, IUseItemModule
    {
        readonly Dictionary<ContextKey, ItemUseContext> _contexts = new();
        uint _useCounter;

        public void Use(Item item)
        {
            var def = ItemManager.GetData(item.Id);
            if (def == null) return;

            ContextKey key = def.IsUniqueContext
                ? new ContextKey(item.Id, ++_useCounter)
                : new ContextKey(item.Id);

            var ctx = new ItemUseContext(Unit, key);
            _contexts.Add(key, ctx);
            def.OnUse(Unit, ctx);
        }

        public void UnUse(Item item)
        {
            var def = ItemManager.GetData(item.Id);
            if (def == null) return;

            var key = FindContextKey(item.Id, def.IsUniqueContext);
            if (!_contexts.TryGetValue(key, out var ctx)) return;

            def.OnUnUse(Unit, ctx);
            ctx.Dispose();
            _contexts.Remove(key);
        }

        ContextKey FindContextKey(int typeId, bool uniqueContext)
        {
            if (!uniqueContext) return new ContextKey(typeId);
            foreach (var key in _contexts.Keys)
                if (key.TypeId == typeId) return key;
            return default;
        }

        protected override void OnReset()
        {
            foreach (var ctx in _contexts.Values) ctx.Dispose();
            _contexts.Clear();
            _useCounter = 0;
        }
    }
    
    [Serializable]
    public struct OnUseInjectSystem : ModuleTemplate<OnUseInjectSystemModule>
    {
        
    }
}