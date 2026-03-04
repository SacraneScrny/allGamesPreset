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
        public CancellationToken ModuleToken => _lifecycleCts?.Token ?? CancellationToken.None;
        
        public bool IsAwaken { get; private set; }
        public bool IsStarted { get; private set; }
        public bool IsDisposed { get; private set; }

        public void Awake()
        {
            if (IsAwaken) return;
            if (IsDisposed) return;
            TraceManager.Trace(Unit, $"        Module {GetType().Name} Awake");
            _lifecycleCts = new CancellationTokenSource();
            IsAwaken = true;
            OnAwake();
        }
        protected virtual void OnAwake() { }
        
        public void Start()
        {
            if (IsStarted) return;
            TraceManager.Trace(Unit, $"        Module {GetType().Name} Start");
            IsStarted = true;
            OnStart();
        }
        protected virtual void OnStart() { }
        
        public virtual bool OnDependencyCheck() => true;
        
        public void Dispose()
        {
            if (IsDisposed) return;
            TraceManager.Trace(Unit, $"        Module {GetType().Name} Disposed");
            CancelLifecycle();
            OnDispose();
            IsDisposed = true;
        }
        protected virtual void OnDispose() { }

        public void Reset()
        {
            if (!IsStarted) return;
            if (IsDisposed) return;
            TraceManager.Trace(Unit, $"        Module {GetType().Name} Reseted");
            
            CancelLifecycle();
            _lifecycleCts = new CancellationTokenSource();
            
            IsStarted = false;
            OnReset();
        }
        protected virtual void OnReset() { }
        
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
        
        void CancelLifecycle()
        {
            if (_lifecycleCts != null)
            {
                _lifecycleCts.Cancel();
                _lifecycleCts.Dispose();
                _lifecycleCts = null;
            }
        }
        
        public virtual void OnDrawGizmos() { }
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