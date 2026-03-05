using System;
using System.Collections.Generic;

using Sackrany.Actor.Modules.Modules;
using Sackrany.Actor.Static;
using Sackrany.Actor.Traits.EffectsFeature;
using Sackrany.Actor.UnitMono;

using UnityEngine;

namespace Sackrany.Actor.Traits.Storage.DataBase.Behaviour
{
    public interface IItemBehaviour
    {
        public void OnUse(Unit unit, ItemUseContext ctx);
        public void OnUnUse(Unit unit, ItemUseContext ctx) { }
    }

    [Serializable]
    public class InjectModules : IItemBehaviour
    {
        [SerializeField][SerializeReference][SubclassSelector]
        public ModuleTemplate[] Modules;

        public void OnUse(Unit unit, ItemUseContext ctx)
        {
            foreach (var t in Modules)
                if (unit.Add(t, out var module))
                    ctx.TrackModule(module);
        }
        public void OnUnUse(Unit unit, ItemUseContext ctx) { }
    }   
    [Serializable]
    public class InjectEffects : IItemBehaviour
    {
        [SerializeField][SerializeReference][SubclassSelector]
        public EffectTemplate[] Effects;

        public void OnUse(Unit unit, ItemUseContext ctx)
        {
            unit.Maybe<EffectHandlerModule>(h =>
            {
                foreach (var t in Effects)
                {
                    var e = t.GetInstance();
                    h.ApplyEffect(t);
                    ctx.TrackEffect(e);
                }
            });
        }
    }
    
    public class ItemUseContext
    {
        readonly Unit _unit;
        readonly List<Module> _modules = new();
        readonly List<Effect> _effects = new();

        public ContextKey Key { get; }

        public ItemUseContext(Unit unit, ContextKey key)
        {
            _unit = unit;
            Key = key;
        }

        public void TrackModule(Module m) => _modules.Add(m);
        public void TrackEffect(Effect e) => _effects.Add(e);

        public void Dispose()
        {
            foreach (var m in _modules) _unit.Remove(m.GetType());
            foreach (var e in _effects) _unit.Maybe<EffectHandlerModule>(h => h.RemoveEffect(e));
            _modules.Clear();
            _effects.Clear();
        }
    }
}