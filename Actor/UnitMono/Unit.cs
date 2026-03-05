using System;
using System.Collections.Generic;

using Sackrany.Actor.Managers;
using Sackrany.Actor.Modules.Modules;
using Sackrany.Utils.Hash;
using Sackrany.Utils.Pool.Abstracts;
using Sackrany.Utils.Tracer;
using Sackrany.Variables.ExpandedVariable.Entities;

using UnityEngine;

namespace Sackrany.Actor.UnitMono
{
    public class Unit : MonoBehaviour, IEquatable<Unit>, IPoolable, ITraceable
    {
        public bool DebugTracing;
        public bool IsTracing() => DebugTracing;
        
        [SerializeField] bool WorkByDefault = true;
        public UnitTag Tag;
        public EventBus.EventBus Event;

        [SerializeField] ModulesController Controller;
        public IEnumerable<Module> GetModules() => Controller?.GetModules();
        
        public UnitArchetype Archetype { get; private set; }
        public TeamInfo Team { get; private set; }
        
        public ExpandedFloat TimeFlow { get; private set; }
        public bool IsWorking { get; private set; }
        public bool IsActive => IsWorking && gameObject.activeSelf && gameObject.activeInHierarchy;
        public uint Hash { get; private set; }
        
        bool _isQuitting;
        
        void Awake()
        {
            TraceManager.Trace(this, $"Unit MonoBehaviour Awake()");
            Application.quitting += OnApplicationQuitting;
            
            Hash = SimpleId.Next();
            Tag.Initialize(this);
            
            Event = new EventBus.EventBus();
            
            Archetype = new UnitArchetype(this);
            TimeFlow = new ExpandedFloat(1);
            Team = new TeamInfo();
            
            Controller.FillUnit(this);
            Controller.FillController(Controller);
            
            StartWork();
        }
        void OnApplicationQuitting() => _isQuitting = true;
        void Start()
        {
            TraceManager.Trace(this, $"Unit MonoBehaviour Start()");
            Controller.Start();
        }

        #region MODULES
        public bool Add(ModuleTemplate template, out Module module) => Controller.Add(template, out module);
        public bool Add(ModuleTemplate template) => Controller.Add(template);
        public bool Add(ModuleTemplate[] templates) => Controller.Add(templates);
        
        public bool Has<T>() where T : Module => Controller.Has<T>();
        public bool Has(Type type) => Controller.Has(type);
        public bool Has(ModuleTemplate template) => Controller.Has(template);
        
        public T Get<T>() where T : Module => Controller.Get<T>();
        public Module Get(Type type) => Controller.Get(type); 
        public Module Get(ModuleTemplate template) => Controller.Get(template);
        
        public bool Remove<T>() where T : Module => Controller.Remove<T>();
        public bool Remove<T>(T module) where T : Module => Remove<T>();
        public bool Remove(ModuleTemplate template) => Controller.Remove(template);
        public bool Remove(Type type) => Controller.Remove(type);

        public void RemoveAll() => Controller.RemoveAll();
        
        public bool TryGet<T>(out T result) where T : Module => Controller.TryGet(out result);
        public bool TryGet(Type type, out Module result) => Controller.TryGet(type, out result);
        public bool TryGet(ModuleTemplate template, out Module result) => Controller.TryGet(template, out result);
        #endregion

        #region UPDATE
        public void OnUpdate(float dt)
        {
            if (!IsWorking) return;
            Controller.Update(dt * TimeFlow);
        }
        public void OnFixedUpdate(float dt)
        {
            if (!IsWorking) return;
            Controller.FixedUpdate(dt * TimeFlow);
        }
        public void OnLateUpdate(float dt)
        {
            if (!IsWorking) return;
            Controller.LateUpdate(dt * TimeFlow);
        }
        #endregion

        #region SERIALIZATION
        public bool IsDeserialized {get; private set;}
        public void MarkAsDeserialized()
        {
            TraceManager.Trace(this, $"Unit Deserialized");
            IsDeserialized = true;
        }
        #endregion
        
        public void StartWork()
        {
            if (IsWorking) return;
            TraceManager.Trace(this, $"Unit Start Working");
            IsWorking = true;
            UnitRegisterManager.RegisterUnit(this);
            OnStartWorking?.Invoke(this);
        }
        public void StopWork()
        {
            if (!IsWorking) return;
            TraceManager.Trace(this, $"Unit Stop Working");
            IsWorking = false;
            UnitRegisterManager.UnregisterUnit(this);
            OnStopWorking?.Invoke(this);
        }
        
        public void ResetState()
        {
            if (!Application.isPlaying) return;
            OnReset?.Invoke(this);
            Tag.Reset();
            Event.Reset();
            TimeFlow.Clear();
            Controller.ResetState();
            TraceManager.Trace(this, $"Unit ResetState");
        }
        public void Reinitialize()
        {
            OnRestart?.Invoke(this);
            Tag.Reset();
            Event.Reset();
            TimeFlow.Clear();
            Controller.Reinitialize();
            TraceManager.Trace(this, $"Unit Reinitialized");
        }
        
        public void OnPooled()
        {
            TraceManager.Trace(this, $"Unit Pooled");
            gameObject.SetActive(true);
            Reinitialize();
            if (WorkByDefault) StartWork();
        }
        public void OnReleased()
        {
            TraceManager.Trace(this, $"Unit Released");
            StopWork();
            gameObject.SetActive(false);
        }
        
        public event Action<Unit> OnStartWorking;
        public event Action<Unit> OnStopWorking;
        public event Action<Unit> OnRestart;
        public event Action<Unit> OnReset;

        #region EQUALS
        public bool Equals(Unit other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Hash == other.Hash;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((Unit)obj);
        }
        public override int GetHashCode()
        {
            return unchecked((int)Hash);
        }

        public static bool operator ==(Unit left, Unit right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null)) return false;
            return left.Equals(right);
        }
        public static bool operator !=(Unit left, Unit right)
            => !(left == right);
        #endregion
        
        void OnDestroy()
        {
            if (_isQuitting) return;
            TraceManager.Trace(this, $"Unit Destroyed");
            Controller.Dispose();
            UnitRegisterManager.UnregisterUnit(this);
            Application.quitting -= OnApplicationQuitting;
        }
        
        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Controller.OnDrawGizmos();    
        }
        #endif
    }
}