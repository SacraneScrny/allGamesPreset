using System;
using System.Threading;

using Sackrany.Actor.Base;
using Sackrany.Actor.Static;
using Sackrany.Utils.Tracer;

namespace Sackrany.Actor.Modules.Modules
{
    public abstract class Module : UnitBase, IDisposable
    {
        CancellationTokenSource _lifecycleCts;
        public virtual CancellationToken ModuleToken => CancellationToken.None;
        
        public bool IsAwaken { get; private set; }
        public bool IsStarted { get; private set; }
        public bool IsDisposed { get; private set; }

        public void Awake()
        {
            if (IsAwaken) return;
            if (IsDisposed) return;
            IsAwaken = true;
            OnAwakeInternal();
            OnAwake();
        }
        public void Start()
        {
            if (IsStarted) return;
            OnStartInternal();
            IsStarted = true;
            OnStart();
        }
        
        public virtual bool OnDependencyCheck() => true;
        
        public void Reset()
        {
            if (!IsStarted) return;
            if (IsDisposed) return;
            OnResetInternal();
            IsStarted = false;
            OnReset();
        }
        public void Dispose()
        {
            if (IsDisposed) return;
            OnDisposeInternal();
            OnDispose();
            IsDisposed = true;
        }
        
        public bool Add(ModuleTemplate template, out Module module) => Controller.Add(template, out module);
        public bool Add(ModuleTemplate template) => Controller.Add(template);
        public bool Add(ModuleTemplate[] templates) => Controller.Add(templates);
        
        public bool Remove<T>() where T : Module => Controller.Remove<T>();
        public bool Remove<T>(T module) where T : Module => Remove<T>();
        public bool Remove(ModuleTemplate template) => Controller.Remove(template);
        public bool Remove(Type type) => Controller.Remove(type);

        public void RemoveAll() => Controller.RemoveAll();
        
        public bool Has<T>() where T : Module => Controller.Has<T>();
        public bool Has(Type type) => Controller.Has(type);
        public bool Has(ModuleTemplate template) => Controller.Has(template);
        
        public T Get<T>() where T : Module => Controller.Get<T>();
        public Module Get(Type type) => Controller.Get(type); 
        public Module Get(ModuleTemplate template) => Controller.Get(template);
        
        public bool TryGet<T>(out T result) where T : Module => Controller.TryGet(out result);
        public bool TryGet(Type type, out Module result) => Controller.TryGet(type, out result);
        public bool TryGet(ModuleTemplate template, out Module result) => Controller.TryGet(template, out result);
        
        public bool IsInitialized()
            => IsStarted && !IsDisposed;
        
        public virtual void OnDrawGizmos() { }
        
        private protected virtual void OnAwakeInternal() { }
        private protected virtual void OnStartInternal() { }
        private protected virtual void OnResetInternal() { }
        private protected virtual void OnDisposeInternal() { }
        
        protected virtual void OnAwake() { }
        protected virtual void OnStart() { }
        protected virtual void OnReset() { }
        protected virtual void OnDispose() { }
    }
    public class AsyncModule : Module
    {
        CancellationTokenSource _lifecycleCts;
        public sealed override CancellationToken ModuleToken => _lifecycleCts?.Token ?? CancellationToken.None;

        private protected sealed override void OnAwakeInternal()
            => _lifecycleCts = new CancellationTokenSource();

        private protected sealed override void OnStartInternal()
        {
            if (_lifecycleCts == null || _lifecycleCts.IsCancellationRequested)
                _lifecycleCts = new CancellationTokenSource();
        }

        private protected sealed override void OnResetInternal()
            => _lifecycleCts?.Cancel();

        private protected sealed override void OnDisposeInternal()
        {
            _lifecycleCts?.Cancel();
            _lifecycleCts?.Dispose();
            _lifecycleCts = null;
        }
    }

    public interface ModuleTemplate
    {
        public int GetId();
        public Module GetInstance();
        public Type GetModuleType() => ModuleRegistry.GetTypeById(GetId());
    }
    public interface ModuleTemplate<T> : ModuleTemplate
        where T : Module, new ()
    {
        int ModuleTemplate.GetId() => ModuleRegistry.GetId<T>();
        Module ModuleTemplate.GetInstance() => new T();
        Type ModuleTemplate.GetModuleType() => typeof(T);
    }
}