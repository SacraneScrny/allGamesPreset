using System;
using System.Collections.Generic;
using System.Threading;

using Cysharp.Threading.Tasks;

using Sackrany.Utils;

namespace Sackrany.Actor.Managers
{
    public class UnitCmd : AManager<UnitCmd>
    {
        readonly List<UnitCommand> _commandHandlers = new();
        CancellationToken _destroyToken;
        bool _isRunning;
        
        void Start()
        {
            _destroyToken = this.GetCancellationTokenOnDestroy();
        }

        public class UnitCommand
        {
            public Func<Unit.Unit, bool> cond;
            public Action<Unit.Unit> action;
            public readonly List<Action> callbacks = new();
            public bool completed;
        }
        public class CommandHandle
        {
            readonly UnitCommand _cmd;
            public CommandHandle(UnitCommand cmd) => _cmd = cmd;

            public CommandHandle OnComplete(Action callback)
            {
                if (_cmd.completed) callback?.Invoke();
                else _cmd.callbacks.Add(callback);
                return this;
            }
        }

        async UniTaskVoid ProcessLoop()
        {
            _isRunning = true;

            try
            {
                while (_commandHandlers.Count > 0)
                {
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, _destroyToken);

                    for (int i = _commandHandlers.Count - 1; i >= 0; i--)
                    {
                        var cmd = _commandHandlers[i];

                        if (!UnitRegisterManager.TryGetUnit(cmd.cond, out var unit))
                            continue;

                        if (!unit.IsActive)
                            continue;

                        cmd.action(unit);

                        foreach (var callback in cmd.callbacks)
                            callback?.Invoke();

                        cmd.callbacks.Clear();
                        cmd.completed = true;

                        _commandHandlers.RemoveAt(i);
                    }
                }
            }
            catch (OperationCanceledException) { }

            _isRunning = false;
        }

        public static CommandHandle Execute(Func<Unit.Unit, bool> cond, Action<Unit.Unit> action)
        {
            var cmd = new UnitCommand
            {
                cond = cond,
                action = action
            };

            Instance._commandHandlers.Add(cmd);

            if (!Instance._isRunning)
                Instance.ProcessLoop().Forget();

            return new CommandHandle(cmd);
        }
    }
}