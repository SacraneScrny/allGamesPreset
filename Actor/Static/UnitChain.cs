using System;
using System.Threading;

using Cysharp.Threading.Tasks;

using Sackrany.Actor.Managers;
using Sackrany.Actor.Modules.Modules;
using Sackrany.Actor.UnitMono;

namespace Sackrany.Actor.Static
{
    public readonly struct UnitChain
    {
        readonly Unit _unit;
        readonly bool _valid;

        public UnitChain(Unit unit)
        {
            _unit  = unit;
            _valid = unit != null && unit.IsActive;
        }

        public static UnitChain From(Unit unit) => new(unit);
        public static UnitChain Find(Func<Unit, bool> p) => new(UnitRegisterManager.GetUnit(u => u.IsActive && p(u)));
        public UnitChain FlatMap(Func<Unit, UnitChain> selector)
            => _valid ? selector(_unit) : default;

        public UnitChain Where(Func<Unit, bool> predicate)
            => _valid && predicate(_unit) ? this : default;
        public UnitChain Where<TModule>(Func<TModule, bool> predicate) where TModule : Module
            => _valid && _unit.TryGet(out TModule m) && predicate(m) ? this : default;
        public UnitChain Has<TModule>() where TModule : Module
            => _valid && _unit.Has<TModule>() ? this : default;

        public UnitChain Do(Action<Unit> action)
        {
            if (_valid) action(_unit);
            return this;
        }
        public UnitChain Do<TModule>(Action<TModule> action) where TModule : Module
        {
            if (_valid && _unit.TryGet(out TModule m)) action(m);
            return this;
        }
        public UnitChain Do<TA, TB>(Action<TA, TB> action)
            where TA : Module where TB : Module
        {
            if (_valid && _unit.TryGet(out TA a) && _unit.TryGet(out TB b)) action(a, b);
            return this;
        }
        
        public ModuleChain<TModule> Module<TModule>() where TModule : Module
        {
            if (_valid && _unit.TryGet(out TModule m)) return new ModuleChain<TModule>(m, _unit);
            return default;
        }
        
        public UnitChain Select(Func<Unit, Unit> selector)
            => _valid ? new UnitChain(selector(_unit)) : default;
        public UnitChain Branch(Func<Unit, bool> predicate, Action<UnitChain> onTrue, Action<UnitChain> onFalse = null)
        {
            if (!_valid) return this;
            if (predicate(_unit)) onTrue(this);
            else onFalse?.Invoke(this);
            return this;
        }
        public UnitChain Tap(Action<Unit> action) => Do(action);
        
        public TResult Get<TModule, TResult>(Func<TModule, TResult> selector, TResult fallback = default)
            where TModule : Module
            => _valid && _unit.TryGet(out TModule m) ? selector(m) : fallback;
        public TResult Get<TResult>(Func<Unit, TResult> selector, TResult fallback = default)
            => _valid ? selector(_unit) : fallback;
        public bool TryGet(out Unit unit)
        {
            unit = _valid ? _unit : null;
            return _valid;
        }
        
        public async UniTask<UnitChain> DoAsync(Func<Unit, UniTask> action)
        {
            if (_valid) await action(_unit);
            return this;
        }
        public async UniTask<UnitChain> DoAsync<TModule>(Func<TModule, UniTask> action) where TModule : Module
        {
            if (_valid && _unit.TryGet(out TModule m)) await action(m);
            return this;
        }
        public async UniTask<UnitChain> WaitActive(CancellationToken token = default)
        {
            if (_unit == null) return default;
            var unit = _unit;
            await UniTask.WaitWhile(() => !unit.IsActive, cancellationToken: token);
            return new UnitChain(_unit);
        }

        public bool IsValid => _valid;
        public Unit Value => _valid ? _unit : null;

        public static implicit operator bool(UnitChain chain) => chain._valid;
        public static implicit operator Unit(UnitChain chain) => chain.Value;
        public static implicit operator UnitChain(Unit unit) => new(unit);
    }
    
    public readonly struct ModuleChain<TModule> where TModule : Module
    {
        readonly TModule _module;
        readonly Unit _unit;
        readonly bool _valid;

        internal ModuleChain(TModule module, Unit unit)
        {
            _module = module;
            _unit = unit;
            _valid = module != null;
        }

        public ModuleChain<TModule> Where(Func<TModule, bool> predicate)
            => _valid && predicate(_module) ? this : default;
        public ModuleChain<TModule> Do(Action<TModule> action)
        {
            if (_valid) action(_module);
            return this;
        }
        public ModuleChain<TModule> Do(Action<TModule, Unit> action)
        {
            if (_valid) action(_module, _unit);
            return this;
        }

        public TResult Get<TResult>(Func<TModule, TResult> selector, TResult fallback = default)
            => _valid ? selector(_module) : fallback;
        public bool TryGet(out TModule module) { module = _valid ? _module : default; return _valid; }

        public ModuleChain<TOther> Switch<TOther>() where TOther : Module
            => _valid ? new UnitChain(_unit).Module<TOther>() : default;
        public UnitChain Back() => _valid ? UnitChain.From(_unit) : default;

        public async UniTask<ModuleChain<TModule>> DoAsync(Func<TModule, UniTask> action)
        {
            if (_valid) await action(_module);
            return this;
        }

        public bool IsValid => _valid;

        public static implicit operator bool(ModuleChain<TModule> c) => c._valid;
    }
}