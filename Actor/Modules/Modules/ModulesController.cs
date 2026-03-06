using System;
using System.Collections.Generic;
using System.Linq;

using Sackrany.Actor.Base;
using Sackrany.Actor.Modules.ModuleComposition;
using Sackrany.Actor.Static;
using Sackrany.Utils.Tracer;

using UnityEngine;

namespace Sackrany.Actor.Modules.Modules
{
    [Serializable]
    [TraceAll]
    public sealed class ModulesController : UnitBase, IDisposable
    {
        public ControllerMode Mode = ControllerMode.Dynamic;
        bool IsDynamic => Mode == ControllerMode.Dynamic;
        
        [SerializeField][SerializeReference][SubclassSelector] 
        public ModuleTemplate[] Default;
        
        public bool IsStarted { get; private set; }
        public bool IsDisposed { get; private set; }
        public IEnumerable<Module> GetModules() => _modules.Values;
        
        public void Start()
        {
            if (IsStarted) return;
            IsStarted = true;
            Add(Default);
        }

        #region MODULES
        readonly Dictionary<int, Module> _modules = new Dictionary<int, Module>();

        public bool Add(ModuleTemplate template, out Module result)
        {
            if (Add(template))
            {
                result = Get(template);
                return true;
            }
            result = null;
            return false;
        }
        public bool Add(ModuleTemplate template)
        {
            if (!IsDynamic) return false;

            if (!CreateAndRegister(template, out var instance))
                return true;

            if (!DependencyCheck(instance))
            {
                RemoveInternal(template.GetId());
                return false;
            }

            instance.Awake();
            ActivateModule(instance);
            return true;
        }
        public bool Add(ModuleTemplate[] templates)
        {
            if (templates.Length == 0) return true;
            if (!IsDynamic && _modules.Count > 0) return false;

            bool allAdded = true;
            templates = templates.OrderBy(x => ModuleReflectionCache.GetMetadata(x.GetModuleType()).UpdateOrder).ToArray();

            var tempModules = new List<(Module module, int id)>(templates.Length);
            for (int i = 0; i < templates.Length; i++)
            {
                if (!CreateAndRegister(templates[i], out var instance))
                {
                    allAdded = false;
                    continue;
                }
                tempModules.Add((instance, templates[i].GetId()));
            }

            bool dependenciesSolved = false;
            while (!dependenciesSolved && _modules.Count > 0 && tempModules.Count > 0)
            {
                dependenciesSolved = true;
                for (int i = tempModules.Count - 1; i >= 0; i--)
                {
                    if (DependencyCheck(tempModules[i].module)) continue;
                    dependenciesSolved = false;
                    RemoveInternal(tempModules[i].id);
                    tempModules.RemoveAt(i);
                    allAdded = false;
                }
            }

            for (int i = 0; i < tempModules.Count; i++)
                tempModules[i].module.Awake();
            for (int i = 0; i < tempModules.Count; i++)
                ActivateModule(tempModules[i].module);

            return allAdded;
        }
        bool CreateAndRegister(ModuleTemplate template, out Module instance)
        {
            if (_modules.TryGetValue(template.GetId(), out instance))
            {
                OnTryToAddAlreadyExist?.Invoke(instance);
                return false;
            }

            instance = template.GetInstance();
            TemplateFill(instance, template);
            instance.FillUnit(Unit);
            instance.FillController(this);
            _modules.Add(template.GetId(), instance);
            return true;
        }
        void ActivateModule(Module instance)
        {
            if (instance is IUpdateModule u)      _updateModules.Add(u);
            if (instance is IFixedUpdateModule f) _fixedUpdateModules.Add(f);
            if (instance is ILateUpdateModule l)  _lateUpdateModules.Add(l);
            instance.Start();
            OnModuleAdded?.Invoke(instance);
        }
        
        public bool Remove<T>() where T : Module
        {
            if (!IsDynamic) return false;
            return RemoveInternal(ModuleRegistry.GetId<T>());
        }
        public bool Remove<T>(T module) where T : Module
        {
            if (!IsDynamic) return false;
            return RemoveInternal(ModuleRegistry.GetId(module.GetType()));
        }
        public bool Remove(ModuleTemplate template)
        {
            if (!IsDynamic) return false;
            return RemoveInternal(template.GetId());
        }
        public bool Remove(Type type)
        {
            if (!IsDynamic) return false;
            return RemoveInternal(ModuleRegistry.GetId(type));
        }
        
        bool RemoveInternal(int id)
        {
            if (!_modules.ContainsKey(id))
                return false;

            var toRemove = new List<int>(4) { id };

            for (int i = 0; i < toRemove.Count; i++)
            {
                var removingType = ModuleRegistry.GetTypeById(toRemove[i]);
                foreach (var (moduleId, module) in _modules)
                {
                    if (toRemove.Contains(moduleId)) continue;
                    if (HasNonOptionalDepOn(module, removingType))
                        toRemove.Add(moduleId);
                }
            }

            for (int i = 0; i < toRemove.Count; i++)
                if (_modules.TryGetValue(toRemove[i], out var m))
                    RemoveSingle(toRemove[i], m);

            return true;
        }
        static bool HasNonOptionalDepOn(Module module, Type removedType)
        {
            var deps = ModuleReflectionCache.GetMetadata(module.GetType()).Dependencies;
            for (int i = 0; i < deps.Length; i++)
            {
                if (deps[i].IsOptional) continue;
                var checkType = deps[i].IsArray ? deps[i].ElementType : deps[i].FieldType;
                if (checkType != null && checkType.IsAssignableFrom(removedType))
                    return true;
            }
            return false;
        }
        void RemoveSingle(int id, Module instance)
        {
            if (instance is IUpdateModule u)      _updateModules.Remove(u);
            if (instance is IFixedUpdateModule f) _fixedUpdateModules.Remove(f);
            if (instance is ILateUpdateModule l)  _lateUpdateModules.Remove(l);
            _modules.Remove(id);
            OnModuleRemoved?.Invoke(instance);
            instance.Dispose();
        }

        public void RemoveAll()
        {
            if (!IsDynamic) return;
            foreach (var module in _modules.Values)
            {
                OnModuleRemoved?.Invoke(module);
                module.Reset();
                module.Dispose();
            }
            _updateModules.Clear();
            _fixedUpdateModules.Clear();
            _lateUpdateModules.Clear();
            _modules.Clear();
        }
        
        public bool Has<T>() where T : Module
            => _modules.ContainsKey(ModuleRegistry.GetId<T>());
        public bool Has(Type type) 
            => _modules.ContainsKey(ModuleRegistry.GetId(type));
        public bool Has(ModuleTemplate template)
            => _modules.ContainsKey(template.GetId());
        
        public T Get<T>() where T : Module
        {
            if (_modules.TryGetValue(ModuleRegistry.GetId<T>(), out var instance))
                return (T)instance;
            return GetAssignable<T>();
        }
        public Module Get(Type type)
        {
            if (_modules.TryGetValue(ModuleRegistry.GetId(type), out var instance))
                return instance;
            return GetAssignable(type);
        }
        public Module Get(ModuleTemplate template)
        {
            if (_modules.TryGetValue(template.GetId(), out var instance))
                return instance;
            return null;
        }
        
        public T GetAssignable<T>() where T : Module
        {
            foreach (var module in _modules.Values)
                if (module is T t)
                    return t;
            return null;
        }
        public Module GetAssignable(Type type)
        {
            foreach (var module in _modules.Values)
                if (type.IsAssignableFrom(module.GetType()))
                    return module;
            return null;
        }
        public Module[] GetAllAssignable(Type type)
        {
            var modules = new List<Module>();
            foreach (var module in _modules.Values)
                if (type.IsAssignableFrom(module.GetType()))
                    modules.Add(module);
            return modules.ToArray();
        }
        
        public bool TryGet<T>(out T result, bool tryAssignable = false) where T : Module
        {
            if (_modules.TryGetValue(ModuleRegistry.GetId<T>(), out var module))
            {
                result = (T)module;
                return true;
            }
            if (tryAssignable && TryGetAssignable<T>(out var resultAssignable))
            {
                result = (T)resultAssignable;
                return true;
            }
            result = null;
            return false;
        }
        public bool TryGet(Type type, out Module result, bool tryAssignable = false)
        {
            if (_modules.TryGetValue(ModuleRegistry.GetId(type), out var module))
            {
                result = module;
                return true;
            }
            if (tryAssignable && TryGetAssignable(type, out var resultAssignable))
            {
                result = resultAssignable;
                return true;
            }
            result = null;
            return false;
        }
        public bool TryGet(ModuleTemplate template, out Module result)
        {
            if (_modules.TryGetValue(template.GetId(), out var module))
            {
                result = module;
                return true;
            }
            result = null;
            return false;
        }
        public bool TryGetAssignable<T>(out Module result)
        {
            foreach (var module in _modules.Values.Where(module => module is T))
            {
                result = module;
                return true;
            }
            result = null;
            return false;
        }
        public bool TryGetAssignable(Type type, out Module result)
        {
            foreach (var module in _modules.Values.Where(module => type.IsAssignableFrom(module.GetType())))
            {
                result = module;
                return true;
            }
            result = null;
            return false;
        }

        bool DependencyCheck(Module m)
        {
            var meta = ModuleReflectionCache.GetMetadata(m.GetType());

            foreach (var dep in meta.Dependencies)
            {
                if (dep.IsArray)
                {
                    if (dep.ElementType == null || !dep.ElementType.IsInterface)
                        return false;

                    var modules = GetAllAssignable(dep.ElementType);

                    if ((modules == null || modules.Length == 0) && !dep.IsOptional)
                        return false;

                    var array = Array.CreateInstance(dep.ElementType, modules.Length);
                    for (int i = 0; i < modules.Length; i++)
                        array.SetValue(modules[i], i);

                    dep.Field.SetValue(m, array);
                }
                else
                {
                    if (!TryResolveDependencies(dep.FieldType, out var resolved))
                    {
                        if (dep.IsOptional) continue;
                        return false;
                    }
                    dep.Field.SetValue(m, resolved);
                }
            }

            return m.OnDependencyCheck();
        }
        bool TryResolveDependencies(Type type, out object result)
        {
            if (TryGet(type, out var module, true))
            {
                result = module;
                return true;
            }

            if (typeof(Component).IsAssignableFrom(type))
            {
                var comp = Unit.GetComponent(type);
                if (comp != null) { result = comp; return true; }

                comp = Unit.GetComponentInChildren(type);
                if (comp != null) { result = comp; return true; }
            }

            result = null;
            return false;
        }
        void TemplateFill(Module m, object template)
        {
            var meta = ModuleReflectionCache.GetMetadata(m.GetType());
            if (meta.Template.Field == null)
                return;
            
            var templateType = template.GetType();
            if (meta.Template.FieldType == templateType)
                meta.Template.Field.SetValue(m, template);
        }
        #endregion

        #region UPDATE
        readonly List<IUpdateModule> _updateModules = new List<IUpdateModule>();
        readonly List<IFixedUpdateModule> _fixedUpdateModules = new List<IFixedUpdateModule>();
        readonly List<ILateUpdateModule> _lateUpdateModules = new List<ILateUpdateModule>();

        public void Update(float deltaTime)
        {
            if (!IsStarted || IsDisposed) return;
            for (int i = 0; i < _updateModules.Count; i++)
                _updateModules[i].OnUpdate(deltaTime);
        }
        public void FixedUpdate(float deltaTime)
        {
            if (!IsStarted || IsDisposed) return;
            for (int i = 0; i < _fixedUpdateModules.Count; i++)
                _fixedUpdateModules[i].OnFixedUpdate(deltaTime);
        }
        public void LateUpdate(float deltaTime)
        {
            if (!IsStarted || IsDisposed) return;
            for (int i = 0; i < _lateUpdateModules.Count; i++)
                _lateUpdateModules[i].OnLateUpdate(deltaTime);
        }
        #endregion
        
        /// <summary>
        /// Complete reset and reinitialization of default modules
        /// </summary>
        public void Reinitialize()
        {
            if (IsDisposed) return;
            if (!IsDynamic)
            {
                ResetState();
                return;
            }
            RemoveAll();
            IsStarted = false;
            OnModuleAdded = null;
            OnModuleRemoved = null;
            OnTryToAddAlreadyExist = null;
            OnModulesRestarted?.Invoke();
            Start();
        }
        
        /// <summary>
        /// Just reset the modules, no reassembly
        /// </summary>
        public void ResetState()
        {
            if (!IsStarted) return;
            if (IsDisposed) return;
            foreach (var module in _modules.Values)
                module.Reset();
            OnModuleAdded = null;
            OnModuleRemoved = null;
            OnTryToAddAlreadyExist = null;
            OnModulesReset?.Invoke();
            foreach (var module in _modules.Values)
                module.Start();
        }
        
        public void Dispose()
        {
            if (IsDisposed) return;
            RemoveAll();
            OnModuleAdded = null;
            OnModuleRemoved = null;
            OnTryToAddAlreadyExist = null;
            IsDisposed = true;
        }
        
        public event Action<Module> OnModuleAdded;
        public event Action OnModulesRestarted;
        public event Action OnModulesReset;
        public event Action<Module> OnTryToAddAlreadyExist;
        public event Action<Module> OnModuleRemoved;

        #if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            foreach (var module in _modules.Values)
                module.OnDrawGizmos();
        }
        #endif
    }

    public enum ControllerMode
    {
        [InspectorName("Dynamic (Add/Remove enabled)")] Dynamic,
        [InspectorName("Sealed (Reset only)")]          Sealed
    }
}