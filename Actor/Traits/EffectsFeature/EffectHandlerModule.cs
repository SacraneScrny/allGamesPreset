using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Cysharp.Threading.Tasks;

using Sackrany.Actor.Modules;
using Sackrany.Actor.Modules.ModuleComposition;
using Sackrany.Actor.Modules.Modules;
using Sackrany.Actor.Traits.Affinity;
using Sackrany.Actor.Traits.EffectsFeature.Static;

using UnityEngine;

namespace Sackrany.Actor.Traits.EffectsFeature
{
    public class EffectHandlerModule : Module, IUpdateModule, IFixedUpdateModule
    {
        [Template] EffectHandler _template;
        
        readonly Dictionary<int, Effect> _effects = new ();
        readonly Dictionary<int, HashSet<Coroutine>> _effectCoroutines = new ();
        readonly Dictionary<int, List<Effect>> _effectByAffinity = new ();
        readonly Dictionary<int, List<(UniTask task, CancellationTokenSource cts)>> _effectTasks = new ();
        
        public float deltaTime { get; private set; }
        public float fixedDeltaTime { get; private set; }
        
        protected override void OnStart()
        {
            ApplyEffects(_template.Default);
        }

        #region Effect
        public bool ApplyEffects(EffectTemplate[] effects)
        {
            bool allApplied = true;
            foreach (var effect in effects)
            {
                allApplied &= ApplyEffect(effect);
            }
            return allApplied;
        }
        
        public bool ApplyEffect<T>(int amount = 1) where T : EffectTemplate, new ()
            => ApplyEffect(new T(), amount);
        public bool ApplyEffect(EffectTemplate effect, int amount = 1)
        {
            if (!Unit.IsActive) return false;
            if (_effects.TryGetValue(effect.GetId(), out var e))
            {
                e.IncreaseAmount(amount);
                return true;
            }

            var instance = effect.GetInstance();
            if (IsCounteredBy(instance.Affinity))
            {
                instance.Dispose();
                return false;
            }
            SolveCounterProblems(instance.Affinity);
            
            _effects.Add(effect.GetId(), instance);
            instance.Initialize(this, amount);
            
            if (!_effectByAffinity.TryGetValue(instance.Affinity.Id, out var tags))
            {
                tags = new List<Effect>();
                _effectByAffinity.Add(instance.Affinity.Id, tags);
            }
            tags.Add(instance);
            return true;
        }
        
        public bool RemoveEffect<T>() where T : Effect
        {
            if (!Unit.IsActive) return false;
            if (!_effects.TryGetValue(EffectRegistry.GetId<T>(), out var e))
                return false;
            if (_effectByAffinity.TryGetValue(e.Affinity.Id, out var tags))
            {
                tags.Remove(e);
                if (tags.Count == 0) _effectByAffinity.Remove(e.Affinity.Id);
            }
            CancelAllEffectTasks<T>();
            _effects.Remove(EffectRegistry.GetId<T>());
            e.Dispose();
            return true;
        }
        public bool RemoveEffect<T>(T effect) where T : Effect
        {
            if (!Unit.IsActive) return false;
            if (!_effects.TryGetValue(effect.Id, out var e))
                return false;
            if (_effectByAffinity.TryGetValue(e.Affinity.Id, out var tags))
            {
                tags.Remove(e);
                if (tags.Count == 0) _effectByAffinity.Remove(e.Affinity.Id);
            }
            CancelAllEffectTasks(effect);
            _effects.Remove(effect.Id);
            e.Dispose();
            return true;
        }
        public bool RemoveAllEffects()
        {
            if (!Unit.IsActive) return false;
            foreach (var effect in _effects.Values)
            {
                if (_effectByAffinity.TryGetValue(effect.Affinity.Id, out var tags))
                {
                    tags.Remove(effect);
                    if (tags.Count == 0) _effectByAffinity.Remove(effect.Affinity.Id);
                }
                CancelAllEffectTasks(effect);
                effect.Dispose();
            }
            _effects.Clear();
            return true;
        }

        public bool ChangeEffectAmount<T>(int offset) where T : Effect
        {
            if (!Unit.IsActive) return false;
            if (!_effects.TryGetValue(EffectRegistry.GetId<T>(), out var e))
                return false;
            switch (offset)
            {
                case 0:
                    return true;
                case > 0:
                    e.IncreaseAmount(offset);
                    break;
                case < 0:
                    e.DecreaseAmount(offset);
                    break;
            }
            return true;
        }
        public bool ChangeEffectAmount<T>(T effect, int offset) where T : Effect
        {
            if (!Unit.IsActive) return false;
            if (!_effects.TryGetValue(effect.Id, out var e))
                return false;
            switch (offset)
            {
                case 0:
                    return true;
                case > 0:
                    e.IncreaseAmount(offset);
                    break;
                case < 0:
                    e.DecreaseAmount(offset);
                    break;
            }
            return true;
        }
        
        public IReadOnlyList<Effect> GetEffectsByAffinity(IAffinity tag) 
            => _effectByAffinity.TryGetValue(tag.Id, out var list) ? list : Array.Empty<Effect>();
        public bool HasEffectsByAffinity(IAffinity tag) => _effectByAffinity.ContainsKey(tag.Id);
        
        bool IsCounteredBy(IAffinity tag)
        {
            if (tag.Id == Affinity<Default>.Id) return false;
            var countered = tag.CounteredBy;
            for (int i = 0; i < countered.Count; i++)
                if (HasEffectsByAffinity(countered[i]))
                    return true;
            return false;
        }
        public void SolveCounterProblems(IAffinity tag)
        {
            if (tag.Id == Affinity<Default>.Id) return;
            var counter = tag.Counter;
            for (int i = 0; i < counter.Count; i++)
            {
                if (!HasEffectsByAffinity(counter[i])) continue;
                var effects = GetEffectsByAffinity(counter[i]);
                for (int j = effects.Count - 1; j >= 0; j--)
                    RemoveEffect(effects[j]);
            }
        }
        #endregion

        #region Tasks
        public CancellationTokenSource StartEffectTask(Effect effect, Func<CancellationToken, UniTask> taskFactory)
        {
            if (!Unit.IsActive) return null;
            
            var cts = new CancellationTokenSource();
            var trackedTask = TrackTaskCompletion(effect.Id, taskFactory, cts);
            trackedTask.Forget();

            if (!_effectTasks.TryGetValue(effect.Id, out var tasks))
            {
                tasks = new ();
                _effectTasks.Add(effect.Id, tasks);
            }
            tasks.Add((trackedTask, cts));
            return cts;
        }
        public CancellationTokenSource StartEffectTask<T>(Func<CancellationToken, UniTask> taskFactory) where T : Effect
        {
            if (!Unit.IsActive) return null;

            var id = EffectRegistry.GetId<T>();
            var cts = new CancellationTokenSource();
            var trackedTask = TrackTaskCompletion(id, taskFactory, cts);
            trackedTask.Forget();

            if (!_effectTasks.TryGetValue(id, out var tasks))
            {
                tasks = new ();
                _effectTasks.Add(id, tasks);
            }
            tasks.Add((trackedTask, cts));
            return cts;
        }

        async UniTask TrackTaskCompletion(
            int effectId, 
            Func<CancellationToken, UniTask> taskFactory, 
            CancellationTokenSource cts)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);
            try
            {
                await taskFactory(linkedCts.Token);
            }
            catch (OperationCanceledException)
            {
                
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if (_effectTasks.TryGetValue(effectId, out var tasks))
                {
                    for (int i = tasks.Count - 1; i >= 0; i--)
                        if (tasks[i].cts == cts) tasks.RemoveAt(i);
                    if (tasks.Count == 0) _effectTasks.Remove(effectId);
                }
                cts?.Dispose();
            }
        }

        public void CancelAllEffectTasks(Effect effect)
        {
            if (!Unit.IsActive) return;
            if (_effectTasks.TryGetValue(effect.Id, out var tasks))
            {
                for (int i = tasks.Count - 1; i >= 0; i--)
                {
                    var task = tasks[i];
                    if (task.cts != null && !task.cts.IsCancellationRequested) 
                        task.cts.Cancel();
                }
            }
        }
        public void CancelAllEffectTasks<T>() where T : Effect
        {
            if (!Unit.IsActive) return;
            var id = EffectRegistry.GetId<T>();
            if (_effectTasks.TryGetValue(id, out var tasks))
            {
                for (int i = tasks.Count - 1; i >= 0; i--)
                {
                    var task = tasks[i];
                    if (task.cts != null && !task.cts.IsCancellationRequested) 
                        task.cts.Cancel();
                }
            }
        }

        public void CancelAllTasks()
        {
            if (!Unit.IsActive) return;
    
            var allTasksLists = _effectTasks.Values.ToArray();
            foreach (var taskList in allTasksLists)
            {
                var tasksCopy = taskList.ToArray();
                foreach (var value in tasksCopy)
                {
                    if (value.cts != null && !value.cts.IsCancellationRequested)
                    {
                        value.cts.Cancel();
                    }
                }
            }
        }
        #endregion

        #region Get
        public T GetEffect<T>() where T : Effect
        {
            if (!_effects.TryGetValue(EffectRegistry.GetId<T>(), out var e) || e.IsDisposed)
                return null;
            return e as T;
        }
        public bool TryGetEffect<T>(out T effect) where T : Effect
        {
            if (!_effects.TryGetValue(EffectRegistry.GetId<T>(), out var e) || e.IsDisposed)
            {
                effect = null;
                return false;
            }
            effect = e as T;
            return true;
        }
        public bool HasEffect<T>() where T : Effect
        {
            return _effects.TryGetValue(EffectRegistry.GetId<T>(), out var e) && !e.IsDisposed;
        }
        #endregion
        
        protected override void OnDispose()
        {
            RemoveAllEffects();
        }
        protected override void OnReset()
        {
            RemoveAllEffects();
        }
        
        public void OnUpdate(float dt)
        {
            deltaTime = dt;
        }
        public void OnFixedUpdate(float dt)
        {
            fixedDeltaTime = dt;
        }
    }
    
    [Serializable]
    public struct EffectHandler : ModuleTemplate<EffectHandlerModule>
    {
        [SerializeField][SerializeReference][SubclassSelector] 
        public EffectTemplate[] Default;
    }
}