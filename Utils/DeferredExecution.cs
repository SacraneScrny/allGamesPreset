using System;
using System.Threading;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace Sackrany.Utils
{
    public static class DeferredExecution
    {
        public static void Execute(GameObject @object, Func<GameObject, bool> condition)
        {
            if (condition(@object))
            {
                @object.SetActive(!@object.activeSelf);
                return;
            }
            WaitUntilAsync(@object, condition, (x) => x.SetActive(!x.activeSelf)).Forget();
        }

        public static void Execute<T>(T @object, Func<T, bool> condition, Action<T> onCond)
        {
            if (condition(@object))
            {
                onCond(@object);
                return;
            }
            WaitUntilAsync(@object, condition, onCond).Forget();
        }

        /// <summary> Выполнить action на следующем кадре. </summary>
        public static void NextFrame(Action action) =>
            NextFrameAsync(action).Forget();

        /// <summary> Выполнить action через N кадров. </summary>
        public static void AfterFrames(int frames, Action action) =>
            AfterFramesAsync(frames, action).Forget();

        /// <summary> Выполнить action через delay секунд. </summary>
        public static void AfterDelay(float delay, Action action) =>
            AfterDelayAsync(delay, action).Forget();

        /// <summary> Выполнить action через delay секунд (игнорирует TimeScale). </summary>
        public static void AfterDelayRealtime(float delay, Action action) =>
            AfterDelayAsync(delay, action, ignoreTimeScale: true).Forget();

        /// <summary>
        /// Ждёт condition, но не дольше timeoutSeconds.
        /// Вызывает onSuccess или onTimeout соответственно.
        /// </summary>
        public static void ExecuteWithTimeout<T>(
            T @object,
            Func<T, bool> condition,
            Action<T> onSuccess,
            Action<T> onTimeout,
            float timeoutSeconds)
        {
            if (condition(@object))
            {
                onSuccess(@object);
                return;
            }
            WithTimeoutAsync(@object, condition, onSuccess, onTimeout, timeoutSeconds).Forget();
        }
        
        /// <summary>
        /// Вызывает action каждые intervalSeconds секунд, пока condition возвращает true.
        /// </summary>
        public static void RepeatWhile(
            Func<bool> condition,
            Action action,
            float intervalSeconds,
            CancellationToken ct = default)
        {
            RepeatWhileAsync(condition, action, intervalSeconds, ct).Forget();
        }

        /// <summary> Вызывает action ровно count раз с паузой intervalSeconds. </summary>
        public static void Repeat(
            int count,
            float intervalSeconds,
            Action<int> action,
            CancellationToken ct = default)
        {
            RepeatAsync(count, intervalSeconds, action, ct).Forget();
        }

        /// <summary> Execute с поддержкой отмены. </summary>
        public static void Execute<T>(
            T @object,
            Func<T, bool> condition,
            Action<T> onCond,
            CancellationToken ct)
        {
            if (condition(@object))
            {
                onCond(@object);
                return;
            }
            WaitUntilAsync(@object, condition, onCond, ct).Forget();
        }

        static async UniTaskVoid WaitUntilAsync<T>(T @object, Func<T, bool> condition, Action<T> onCond,
            CancellationToken ct = default)
        {
            await UniTask.WaitUntil(() => condition(@object), cancellationToken: ct);
            onCond(@object);
        }
        static async UniTaskVoid NextFrameAsync(Action action)
        {
            await UniTask.NextFrame();
            action();
        }
        static async UniTaskVoid AfterFramesAsync(int frames, Action action)
        {
            await UniTask.DelayFrame(frames);
            action();
        }
        static async UniTaskVoid AfterDelayAsync(float delay, Action action, bool ignoreTimeScale = false)
        {
            var timing = ignoreTimeScale ? PlayerLoopTiming.Update : PlayerLoopTiming.Update;
            await UniTask.Delay(
                TimeSpan.FromSeconds(delay),
                ignoreTimeScale: ignoreTimeScale,
                delayTiming: timing);
            action();
        }
        static async UniTaskVoid WithTimeoutAsync<T>(
            T @object,
            Func<T, bool> condition,
            Action<T> onSuccess,
            Action<T> onTimeout,
            float timeoutSeconds)
        {
            bool completed = await UniTask
                .WaitUntil(() => condition(@object))
                .TimeoutWithoutException(TimeSpan.FromSeconds(timeoutSeconds));

            if (completed) onSuccess(@object);
            else           onTimeout(@object);
        }
        static async UniTaskVoid RepeatWhileAsync(
            Func<bool> condition,
            Action action,
            float intervalSeconds,
            CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && condition())
            {
                action();
                await UniTask.Delay(TimeSpan.FromSeconds(intervalSeconds), cancellationToken: ct);
            }
        }
        static async UniTaskVoid RepeatAsync(
            int count,
            float intervalSeconds,
            Action<int> action,
            CancellationToken ct)
        {
            for (int i = 0; i < count && !ct.IsCancellationRequested; i++)
            {
                action(i);
                if (i < count - 1)
                    await UniTask.Delay(TimeSpan.FromSeconds(intervalSeconds), cancellationToken: ct);
            }
        }
    }
}