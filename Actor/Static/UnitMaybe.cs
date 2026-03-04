using System;
using System.Threading;

using Cysharp.Threading.Tasks;

using Sackrany.Actor.Managers;
using Sackrany.Actor.Modules.Modules;

namespace Sackrany.Actor.Static
{
    public static class UnitMaybe
    {
        public static bool Maybe<TModule>(this Unit.Unit unit, Action<TModule> action)
            where TModule : Module
        {
            if (unit == null || !unit.IsActive) return false;
            if (!unit.TryGet(out TModule module)) return false;
            action(module);
            return true;
        }
        public static bool Maybe(this Unit.Unit unit, Action<Unit.Unit> action)
        {
            if (unit == null || !unit.IsActive) return false;
            action(unit);
            return true;
        }

        public static TResult Maybe<TModule, TResult>(this Unit.Unit unit, Func<TModule, TResult> func,
            TResult fallback = default)
            where TModule : Module
        {
            if (unit == null || !unit.IsActive) return fallback;
            if (!unit.TryGet(out TModule module)) return fallback;
            return func(module);
        }

        public static bool MaybeIf<TModule>(this Unit.Unit unit, Func<TModule, bool> predicate, Action<TModule> action)
            where TModule : Module
        {
            if (unit == null || !unit.IsActive) return false;
            if (!unit.TryGet(out TModule module)) return false;
            if (!predicate(module)) return false;
            action(module);
            return true;
        }

        public static void MaybeOr<TModule>(this Unit.Unit unit, Action<TModule> action, Action fallback)
            where TModule : Module
        {
            if (!Maybe(unit, action)) fallback?.Invoke();
        }
        public static void MaybeOr(this Unit.Unit unit, Action<Unit.Unit> action, Action fallback)
        {
            if (!Maybe(unit, action)) fallback?.Invoke();
        }

        public static void Command<TModule>(this Unit.Unit unit, Action<TModule> action)
            where TModule : Module
        {
            if (unit == null) return;
            if (!Maybe<TModule>(unit, action))
                UnitModuleCommand(unit, action, unit.GetCancellationTokenOnDestroy()).Forget();
        }
        public static void Command(this Unit.Unit unit, Action<Unit.Unit> action)
        {
            if (unit == null) return;
            if (!Maybe(unit, action))
                UnitCommand(unit, action, unit.GetCancellationTokenOnDestroy()).Forget();
        }
        public static void Command<TModule>(this Unit.Unit unit, Action<TModule> action, int timeoutMs,
            Action onTimeout = null)
            where TModule : Module
        {
            if (unit == null) return;
            if (!Maybe<TModule>(unit, action))
                UnitModuleCommandTimeout(unit, action, timeoutMs, onTimeout, unit.GetCancellationTokenOnDestroy())
                    .Forget();
        }

        public static async UniTask<bool> MaybeAsync<TModule>(this Unit.Unit unit, Action<TModule> action,
            CancellationToken token = default)
            where TModule : Module
        {
            if (unit == null) return false;
            if (!unit.IsActive)
                await UniTask.WaitWhile(() => unit != null && !unit.IsActive, cancellationToken: token);
            return Maybe<TModule>(unit, action);
        }
        public static async UniTask<bool> MaybeAsync(this Unit.Unit unit, Action<Unit.Unit> action,
            CancellationToken token = default)
        {
            if (unit == null) return false;
            if (!unit.IsActive)
                await UniTask.WaitWhile(() => unit != null && !unit.IsActive, cancellationToken: token);
            return Maybe(unit, action);
        }

        public static bool MaybeFirst<TModule>(Func<Unit.Unit, bool> predicate, Action<TModule> action)
            where TModule : Module
        {
            var unit = UnitRegisterManager.GetUnit(u => u.IsActive && u.Has<TModule>() && predicate(u));
            return unit.Maybe(action);
        }
        public static int MaybeAll<TModule>(Func<Unit.Unit, bool> predicate, Action<TModule> action)
            where TModule : Module
        {
            int count = 0;
            foreach (var unit in UnitRegisterManager.GetAllUnits(
                         u => u.IsActive && u.Has<TModule>() && predicate(u)))
                if (unit.Maybe(action))
                    count++;
            return count;
        }

        static async UniTaskVoid UnitModuleCommand<TModule>(Unit.Unit unit, Action<TModule> action,
            CancellationToken token)
            where TModule : Module
        {
            await UniTask.WaitWhile(() => unit != null && !unit.IsActive, cancellationToken: token);
            Maybe<TModule>(unit, action);
        }
        static async UniTaskVoid UnitCommand(Unit.Unit unit, Action<Unit.Unit> action, CancellationToken token)
        {
            await UniTask.WaitWhile(() => unit != null && !unit.IsActive, cancellationToken: token);
            Maybe(unit, action);
        }
        static async UniTaskVoid UnitModuleCommandTimeout<TModule>(
            Unit.Unit unit, Action<TModule> action,
            int timeoutMs, Action onTimeout,
            CancellationToken token)
            where TModule : Module
        {
            var result = await UniTask
                .WaitWhile(() => unit != null && !unit.IsActive, cancellationToken: token)
                .TimeoutWithoutException(TimeSpan.FromMilliseconds(timeoutMs));

            if (result) onTimeout?.Invoke();
            else Maybe<TModule>(unit, action);
        }
    }
}