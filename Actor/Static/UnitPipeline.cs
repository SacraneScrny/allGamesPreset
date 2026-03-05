using System;
using System.Collections.Generic;

using Sackrany.Actor.Managers;
using Sackrany.Actor.Modules.Modules;
using Sackrany.Actor.UnitMono;

namespace Sackrany.Actor.Static
{
    public sealed class UnitPipeline
    {
        readonly List<Func<Unit, bool>> _steps = new();

        public UnitPipeline Where(Func<Unit, bool> predicate)
        {
            _steps.Add(predicate);
            return this;
        }
        public UnitPipeline Where<TModule>(Func<TModule, bool> predicate) where TModule : Module
        {
            _steps.Add(u => u.TryGet(out TModule m) && predicate(m));
            return this;
        }
        public UnitPipeline Has<TModule>() where TModule : Module
        {
            _steps.Add(u => u.Has<TModule>());
            return this;
        }
        
        public UnitPipeline Do(Action<Unit> action)
        {
            _steps.Add(u => { action(u); return true; });
            return this;
        }
        public UnitPipeline Do<TModule>(Action<TModule> action) where TModule : Module
        {
            _steps.Add(u => {
                if (!u.TryGet(out TModule m)) return true;
                action(m);
                return true;
            });
            return this;
        }

        public bool Execute(Unit unit)
        {
            if (unit == null || !unit.IsActive) return false;
            foreach (var step in _steps)
                if (!step(unit)) return false;
            return true;
        }
        public int Execute(IEnumerable<Unit> units)
        {
            int count = 0;
            foreach (var unit in units)
                if (Execute(unit)) count++;
            return count;
        }
        public int ExecuteAll()
        {
            int count = 0;
            foreach (var unit in UnitRegisterManager.GetAllUnits())
                if (Execute(unit)) count++;
            return count;
        }
    }
}