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
    public sealed class ModulesController : UnitBase, IDisposable
    {
        public bool IsDynamic = true;
        [SerializeField][SerializeReference][SubclassSelector] 
        public ModuleTemplate[] Default;
        
        public bool IsStarted { get; private set; }
        public bool IsDisposed { get; private set; }
        public IEnumerable<Module> GetModules() => _modules.Values;
        
        public void Start()
        {
            if (IsStarted) return;
            TraceManager.Trace(Unit, $"ModulesController Started");
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
            TraceManager.Trace(Unit, $"    Add(ModuleTemplate) trying to add {template.GetType().Name}");
            if (_modules.TryGetValue(template.GetId(), out var module))
            {
                OnTryToAddAlreadyExist?.Invoke(module);
                TraceManager.Trace(Unit, $"    Add(ModuleTemplate) {template.GetType().Name} failed to add, already existing module");
                return true;
            }

            var instance = template.GetInstance();
            TemplateFill(instance, template);
            instance.FillUnit(Unit);
            instance.FillController(this);
            
            _modules.Add(template.GetId(), instance);
            if (!DependencyCheck(instance))
            {
                TraceManager.Trace(Unit, $"    Add(ModuleTemplate) {instance.GetType().Name} failed on dependency check");
                Remove(instance);
                return false;
            }
            
            TraceManager.Trace(Unit, $"    Add(ModuleTemplate) {instance.GetType().Name} successfully added");
            instance.Awake();
            
            if (instance is IUpdateModule updateModule)
            {
                _updateModules.Add(updateModule);
                TraceManager.Trace(Unit, $"    Add(ModuleTemplate) {instance.GetType().Name} is IUpdateModule");
            }
            if (instance is IFixedUpdateModule fixedUpdateModule)
            {
                _fixedUpdateModules.Add(fixedUpdateModule);
                TraceManager.Trace(Unit, $"    Add(ModuleTemplate) {instance.GetType().Name} is IFixedUpdateModule");
            }
            if (instance is ILateUpdateModule lateUpdateModule)
            {
                _lateUpdateModules.Add(lateUpdateModule);
                TraceManager.Trace(Unit, $"    Add(ModuleTemplate) {instance.GetType().Name} is ILateUpdateModule");
            }
            
            instance.Start();
            OnModuleAdded?.Invoke(instance);
            return true;
        }
        public bool Add(ModuleTemplate[] templates)
        {
            if (!IsDynamic && _modules.Count > 0) return false;
            TraceManager.Trace(Unit, $"    Add(ModuleTemplate[]) trying to add {templates.Select(x => x.GetType().Name).Aggregate((x, y) => $"{x}, {y}")}");
            bool allAdded = true;
            templates = templates.OrderBy(x =>
            {
                var meta = ModuleReflectionCache.GetMetadata(x.GetModuleType());
                return meta.UpdateOrder;
            }).ToArray();
            
            List<(Module, int)> tempModules = new List<(Module, int)>();
            for (int i = 0; i < templates.Length; i++)
            {
                if (_modules.TryGetValue(templates[i].GetId(), out var module))
                {
                    allAdded = false;
                    OnTryToAddAlreadyExist?.Invoke(module);
                    TraceManager.Trace(Unit, $"    Add(ModuleTemplate[]) {templates[i].GetType().Name} failed to add, already existing module");
                    continue;
                }
                var instance = templates[i].GetInstance();
                TemplateFill(instance, templates[i]);
                instance.FillUnit(Unit);
                instance.FillController(this);
                
                _modules.Add(templates[i].GetId(), instance);
                tempModules.Add((instance, templates[i].GetId()));
            }

            bool dependenciesSolved = false;
            while (!dependenciesSolved && _modules.Count > 0 && tempModules.Count > 0)
            {
                dependenciesSolved = true;
                for (int i = tempModules.Count - 1; i >= 0; i--)
                {
                    if (!DependencyCheck(tempModules[i].Item1))
                    {
                        dependenciesSolved = false;
                        TraceManager.Trace(Unit, $"    Add(ModuleTemplate[]) {tempModules[i].Item1.GetType().Name} failed on dependency check");
                        Remove(tempModules[i].Item1);
                        tempModules.RemoveAt(i);
                        allAdded = false;
                    }
                }
            }

            for (int i = 0; i < tempModules.Count; i++)
            {
                TraceManager.Trace(Unit, $"    Add(ModuleTemplate[]) {tempModules[i].Item1.GetType().Name} successfully added");
                tempModules[i].Item1.Awake();
            }

            for (int i = 0; i < tempModules.Count; i++)
            {
                if (tempModules[i].Item1 is IUpdateModule updateModule)
                {
                    _updateModules.Add(updateModule);
                    TraceManager.Trace(Unit, $"    Add(ModuleTemplate[]) {tempModules[i].Item1.GetType().Name} is IUpdateModule");
                }
                if (tempModules[i].Item1 is IFixedUpdateModule fixedUpdateModule)
                {
                    _fixedUpdateModules.Add(fixedUpdateModule);
                    TraceManager.Trace(Unit, $"    Add(ModuleTemplate[]) {tempModules[i].Item1.GetType().Name} is IFixedUpdateModule");
                }
                if (tempModules[i].Item1 is ILateUpdateModule lateUpdateModule)
                {
                    _lateUpdateModules.Add(lateUpdateModule);
                    TraceManager.Trace(Unit, $"    Add(ModuleTemplate[]) {tempModules[i].Item1.GetType().Name} is ILateUpdateModule");
                }
                
                tempModules[i].Item1.Start();
                OnModuleAdded?.Invoke(tempModules[i].Item1);
            }
            
            return allAdded;
        }

        public bool Remove<T>() where T : Module
        {
            if (!IsDynamic) return false;
            TraceManager.Trace(Unit, $"    Remove<T> trying to remove {typeof(T).Name}");
            if (!_modules.TryGetValue(ModuleRegistry.GetId<T>(), out var instance))
            {
                TraceManager.Trace(Unit, $"    Remove<T> {typeof(T).Name} failed to remove, does not exists");
                return false;
            }
            
            if (instance is IUpdateModule updateModule)
            {
                var res = _updateModules.Remove(updateModule);
                TraceManager.Trace(Unit, $"    Remove<T> {instance.GetType().Name} was IUpdateModule, remove status: {res}");
            }
            if (instance is IFixedUpdateModule fixedUpdateModule)
            {
                var res = _fixedUpdateModules.Remove(fixedUpdateModule);
                TraceManager.Trace(Unit, $"    Remove<T> {instance.GetType().Name} was IFixedUpdateModule, remove status: {res}");
            }
            if (instance is ILateUpdateModule lateUpdateModule)
            {
                var res = _lateUpdateModules.Remove(lateUpdateModule);
                TraceManager.Trace(Unit, $"    Remove<T> {instance.GetType().Name} was ILateUpdateModule, remove status: {res}");
            }
            
            _modules.Remove(ModuleRegistry.GetId<T>());
            TraceManager.Trace(Unit, $"    Remove<T> {typeof(T).Name} successfully removed");
            OnModuleRemoved?.Invoke(instance);
            instance.Dispose();
            return true;
        }
        public bool Remove<T>(T module) where T : Module
        {
            if (!IsDynamic) return false;
            TraceManager.Trace(Unit, $"    Remove(T) trying to remove {module.GetType().Name}");
            if (!_modules.TryGetValue(ModuleRegistry.GetId(module.GetType()), out var instance))
            {
                TraceManager.Trace(Unit, $"    Remove(T) {module.GetType().Name} failed to remove, does not exists");
                return false;
            }
            
            if (instance is IUpdateModule updateModule)
            {
                var res = _updateModules.Remove(updateModule);
                TraceManager.Trace(Unit, $"    Remove(T) {instance.GetType().Name} was IUpdateModule, remove status: {res}");
            }
            if (instance is IFixedUpdateModule fixedUpdateModule)
            {
                var res = _fixedUpdateModules.Remove(fixedUpdateModule);
                TraceManager.Trace(Unit, $"    Remove(T) {instance.GetType().Name} was IFixedUpdateModule, remove status: {res}");
            }
            if (instance is ILateUpdateModule lateUpdateModule)
            {
                var res = _lateUpdateModules.Remove(lateUpdateModule);
                TraceManager.Trace(Unit, $"    Remove(T) {instance.GetType().Name} was ILateUpdateModule, remove status: {res}");
            }
            
            _modules.Remove(ModuleRegistry.GetId(module.GetType()));
            TraceManager.Trace(Unit, $"    Remove(T) {module.GetType().Name} successfully removed");
            
            OnModuleRemoved?.Invoke(instance);
            instance.Dispose();
            return true;
        }
        public bool Remove(ModuleTemplate template)
        {
            if (!IsDynamic) return false;
            TraceManager.Trace(Unit, $"    Remove(ModuleTemplate) trying to remove {template.GetType().Name}");
            if (!_modules.TryGetValue(template.GetId(), out var instance))
            {
                TraceManager.Trace(Unit, $"    Remove(ModuleTemplate) {template.GetType().Name} failed to remove, does not exists (template)");
                return false;
            }
            
            if (instance is IUpdateModule updateModule)
            {
                var res = _updateModules.Remove(updateModule);
                TraceManager.Trace(Unit, $"    Remove(ModuleTemplate) {instance.GetType().Name} was IUpdateModule, remove status: {res}");
            }
            if (instance is IFixedUpdateModule fixedUpdateModule)
            {
                var res = _fixedUpdateModules.Remove(fixedUpdateModule);
                TraceManager.Trace(Unit, $"    Remove(ModuleTemplate) {instance.GetType().Name} was IFixedUpdateModule, remove status: {res}");
            }
            if (instance is ILateUpdateModule lateUpdateModule)
            {
                var res = _lateUpdateModules.Remove(lateUpdateModule);
                TraceManager.Trace(Unit, $"    Remove(ModuleTemplate) {instance.GetType().Name} was ILateUpdateModule, remove status: {res}");
            }
            
            _modules.Remove(template.GetId());
            TraceManager.Trace(Unit, $"    Remove(ModuleTemplate) {template.GetType().Name} successfully removed");
            
            OnModuleRemoved?.Invoke(instance);
            instance.Dispose();
            return true;
        }
        public bool Remove(Type type)
        {
            if (!IsDynamic) return false;
            TraceManager.Trace(Unit, $"    Remove(Type) trying to remove {type.Name}");
            var id = ModuleRegistry.GetId(type);
            if (!_modules.TryGetValue(id, out var instance))
            {
                TraceManager.Trace(Unit, $"    Remove(Type) {type.Name} failed to remove, does not exists");
                return false;
            }
            
            if (instance is IUpdateModule updateModule)
            {
                var res = _updateModules.Remove(updateModule);
                TraceManager.Trace(Unit, $"    Remove(Type) {instance.GetType().Name} was IUpdateModule, remove status: {res}");
            }
            if (instance is IFixedUpdateModule fixedUpdateModule)
            {
                var res = _fixedUpdateModules.Remove(fixedUpdateModule);
                TraceManager.Trace(Unit, $"    Remove(Type) {instance.GetType().Name} was IFixedUpdateModule, remove status: {res}");
            }
            if (instance is ILateUpdateModule lateUpdateModule)
            {
                var res = _lateUpdateModules.Remove(lateUpdateModule);
                TraceManager.Trace(Unit, $"    Remove(Type) {instance.GetType().Name} was ILateUpdateModule, remove status: {res}");
            }
            
            _modules.Remove(id);
            TraceManager.Trace(Unit, $"    Remove(Type) {type.Name} successfully removed");
            
            OnModuleRemoved?.Invoke(instance);
            instance.Dispose();
            return true;
        }

        public void RemoveAll()
        {
            if (!IsDynamic) return;
            TraceManager.Trace(Unit, $"    RemoveAll() trying to remove All");
            foreach (var module in _modules.Values)
            {
                TraceManager.Trace(Unit, $"    RemoveAll() {module.GetType().Name} successfully removed");
                OnModuleRemoved?.Invoke(module);
                module.Dispose();
            }
            _updateModules.Clear();
            _fixedUpdateModules.Clear();
            _lateUpdateModules.Clear();
            _modules.Clear();
            TraceManager.Trace(Unit, $"    RemoveAll() All modules removed");
        }
        
        public bool Has<T>() where T : Module
            => _modules.ContainsKey(ModuleRegistry.GetId<T>());
        public bool Has(Type type) 
            => _modules.ContainsKey(ModuleRegistry.GetId(type));
        public bool Has(ModuleTemplate template)
            => _modules.ContainsKey(template.GetId());
        
        public T Get<T>() where T : Module
        {
            TraceManager.Trace(Unit, $"    Get<T> tying to retrieve {typeof(T).Name} ");
            if (_modules.TryGetValue(ModuleRegistry.GetId<T>(), out var instance))
            {
                TraceManager.Trace(Unit, $"    Get<T> {instance.GetType().Name} successfully retrieved");
                return (T)instance;
            }
            TraceManager.Trace(Unit, $"    Get<T> {typeof(T).Name} failed to retrieve straight way");
            return GetAssignable<T>();
        }
        public Module Get(Type type)
        {
            TraceManager.Trace(Unit, $"    Get(Type) tying to retrieve {type.Name} ");
            if (_modules.TryGetValue(ModuleRegistry.GetId(type), out var instance))
            {
                TraceManager.Trace(Unit, $"    Get(Type) {instance.GetType().Name} successfully retrieved");
                return instance;
            }
            TraceManager.Trace(Unit, $"    Get(Type) {type.Name} failed to retrieve straight way");
            return GetAssignable(type);
        }
        public Module Get(ModuleTemplate template)
        {
            TraceManager.Trace(Unit, $"    Get(ModuleTemplate) tying to retrieve {template.GetType().Name} ");
            if (_modules.TryGetValue(template.GetId(), out var instance))
            {
                TraceManager.Trace(Unit, $"    Get(ModuleTemplate) {instance.GetType().Name} successfully retrieved");
                return instance;
            }
            TraceManager.Trace(Unit, $"    Get(Type) {template.GetType().Name} failed to retrieve");
            return null;
        }
        
        public T GetAssignable<T>() where T : Module
        {
            TraceManager.Trace(Unit, $"    GetAssignable<T> tying to retrieve {typeof(T).Name}");
            foreach (var module in _modules.Values)
                if (module is T t)
                {
                    TraceManager.Trace(Unit, $"    GetAssignable<T> {module.GetType().Name} successfully retrieved");
                    return t;
                }
            TraceManager.Trace(Unit, $"    GetAssignable<T> failed to retrieve {typeof(T).Name}");
            return null;
        }
        public Module GetAssignable(Type type)
        {
            TraceManager.Trace(Unit, $"    GetAssignable(Type) tying to retrieve {type.Name}");
            foreach (var module in _modules.Values)
                if (type.IsAssignableFrom(module.GetType()))
                {
                    TraceManager.Trace(Unit, $"    GetAssignable(Type) {module.GetType().Name} successfully retrieved");
                    return module;
                }
            TraceManager.Trace(Unit, $"    GetAssignable(Type) failed to retrieve {type.Name}");
            return null;
        }
        public Module[] GetAllAssignable(Type type)
        {
            TraceManager.Trace(Unit, $"    GetAllAssignable(Type) tying to retrieve {type.Name}");
            List<Module> modules = new List<Module>();
            foreach (var module in _modules.Values)
                if (type.IsAssignableFrom(module.GetType()))
                {
                    TraceManager.Trace(Unit, $"    GetAssignable(Type) {module.GetType().Name} found");
                    modules.Add(module);
                }
            TraceManager.Trace(Unit, $"    GetAssignable(Type) retrieve {modules.Count} elements of type {type.Name}");
            return modules.ToArray();
        }
        
        public bool TryGet<T>(out T result, bool tryAssignable = false) where T : Module
        {
            TraceManager.Trace(Unit, $"    TryGet<T> tying to retrieve {typeof(T).Name} ");
            if (_modules.TryGetValue(ModuleRegistry.GetId<T>(), out var module))
            {
                TraceManager.Trace(Unit, $"    TryGet<T> {module.GetType().Name} successfully retrieved");
                result = (T)module;
                return true;
            }
            TraceManager.Trace(Unit, $"    TryGet<T> {typeof(T).Name} failed to retrieve {(tryAssignable ? "straight way" : "")}");
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
            TraceManager.Trace(Unit, $"    TryGet(type) tying to retrieve {type.Name}");
            if (_modules.TryGetValue(ModuleRegistry.GetId(type), out var module))
            {
                TraceManager.Trace(Unit, $"    TryGet(type) {module.GetType().Name} successfully retrieved");
                result = module;
                return true;
            }
            TraceManager.Trace(Unit, $"    TryGet(type) {type.Name} failed to retrieve {(tryAssignable ? "straight way" : "")}");
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
            TraceManager.Trace(Unit, $"    TryGet(ModuleTemplate) tying to retrieve {template.GetType().Name}");
            if (_modules.TryGetValue(template.GetId(), out var module))
            {
                TraceManager.Trace(Unit, $"    TryGet(ModuleTemplate) {module.GetType().Name} successfully retrieved");
                result = module;
                return true;
            }
            TraceManager.Trace(Unit, $"    TryGet(type) {template.GetType().Name} failed to retrieve");
            result = null;
            return false;
        }
        public bool TryGetAssignable<T>(out Module result)
        {
            TraceManager.Trace(Unit, $"    TryGetAssignable<T> tying to retrieve {typeof(T).Name}");
            foreach (var module in _modules.Values.Where(module => module is T))
            {
                TraceManager.Trace(Unit, $"    TryGetAssignable<T> {module.GetType().Name} successfully retrieved");
                result = module;
                return true;
            }
            TraceManager.Trace(Unit, $"    TryGetAssignable<T> {typeof(T).Name} failed to retrieve");
            result = null;
            return false;
        }
        public bool TryGetAssignable(Type type, out Module result)
        {
            TraceManager.Trace(Unit, $"    TryGetAssignable(Type) tying to retrieve {type.Name}");
            foreach (var module in _modules.Values.Where(module => type.IsAssignableFrom(module.GetType())))
            {
                TraceManager.Trace(Unit, $"    TryGetAssignable(Type) {module.GetType().Name} successfully retrieved");
                result = module;
                return true;
            }
            TraceManager.Trace(Unit, $"    TryGetAssignable(Type) {type.Name} failed to retrieve");
            result = null;
            return false;
        }

        bool DependencyCheck(Module m)
        {
            TraceManager.Trace(Unit,
                $"        DependencyCheck(Module) try to check dependencies for {m.GetType().Name}");

            var meta = ModuleReflectionCache.GetMetadata(m.GetType());

            foreach (var dep in meta.Dependencies)
            {
                TraceManager.Trace(Unit,
                    $"        DependencyCheck(Module) {m.GetType().Name} [Dependency] {dep.FieldType.Name}");

                if (dep.IsArray)
                {
                    TraceManager.Trace(Unit,
                        $"        DependencyCheck(Module) {m.GetType().Name} [Dependency] interface[] {dep.FieldType.Name}");

                    if (dep.ElementType == null || !dep.ElementType.IsInterface)
                    {
                        TraceManager.Trace(Unit,
                            $"        DependencyCheck(Module) {m.GetType().Name} [Dependency] Array dependency must be interface[] : {dep.FieldType}");
                        return false;
                    }

                    var modules = GetAllAssignable(dep.ElementType);

                    if ((modules == null || modules.Length == 0) && !dep.IsOptional)
                    {
                        TraceManager.Trace(Unit,
                            $"        DependencyCheck(Module) {m.GetType().Name} [Dependency] Module failed to resolve {dep.ElementType.Name}[] dependency");
                        return false;
                    }

                    var array = Array.CreateInstance(dep.ElementType, modules.Length);
                    for (int i = 0; i < modules.Length; i++)
                    {
                        TraceManager.Trace(Unit,
                            $"        DependencyCheck(Module) el: {modules[i].GetType().Name}[{i}]");
                        array.SetValue(modules[i], i);
                    }

                    dep.Field.SetValue(m, array);
                    TraceManager.Trace(Unit,
                        $"        DependencyCheck(Module) {m.GetType().Name} [Dependency] {dep.ElementType.Name}[] successfully resolved {dep.FieldType.Name} ({modules.Length} elements)");
                }
                else
                {
                    TraceManager.Trace(Unit,
                        $"        DependencyCheck(Module) {m.GetType().Name} [Dependency] single field {dep.FieldType.Name}");

                    if (!TryResolveDependencies(dep.FieldType, out var resolved))
                    {
                        TraceManager.Trace(Unit,
                            $"        DependencyCheck(Module) {m.GetType().Name} [Dependency] Module failed to resolve{(dep.IsOptional ? " OPTIONAL" : "")} {dep.FieldType.Name} dependency");
                        if (dep.IsOptional) continue;
                        return false;
                    }

                    TraceManager.Trace(Unit,
                        $"        DependencyCheck(Module) {m.GetType().Name} [Dependency] {m.GetType().Name} successfully resolved with {resolved.GetType().Name} (field {dep.FieldType.Name})");
                    dep.Field.SetValue(m, resolved);
                }
            }

            return m.OnDependencyCheck();
        }
        bool TryResolveDependencies(Type type, out object result)
        {
            TraceManager.Trace(Unit, $"            TryResolveDependencies(Type) trying to resolve {type.Name}");
            if (TryGet(type, out var module, true))
            {
                TraceManager.Trace(Unit, $"            TryResolveDependencies(Type) successfully resolved {module.GetType().Name} (Module)");
                result = module;
                return true;
            }

            if (typeof(Component).IsAssignableFrom(type))
            {
                var comp = Unit.GetComponent(type);
                if (comp != null)
                {
                    TraceManager.Trace(Unit, $"            TryResolveDependencies(Type) successfully resolved {comp.GetType().Name} (Component In Parent)");
                    result = comp;
                    return true;
                }
                comp = Unit.GetComponentInChildren(type);
                if (comp != null)
                {
                    TraceManager.Trace(Unit, $"            TryResolveDependencies(Type) successfully resolved {comp.GetType().Name} (Component In Children)");
                    result = comp;
                    return true;
                }
            }
            TraceManager.Trace(Unit, $"            TryResolveDependencies(Type) failed to resolve {type.Name}");

            result = null;
            return false;
        }
        void TemplateFill(Module m, object template)
        {
            TraceManager.Trace(Unit, $"        TemplateFill(Module) trying to fill {template.GetType().Name}");

            var meta = ModuleReflectionCache.GetMetadata(m.GetType());
            if (meta.Template.Field == null)
            {
                TraceManager.Trace(Unit, $"        TemplateFill(Module) {m.GetType().Name} has no [Template] field, skipping");
                return;
            }
            
            var templateType = template.GetType();
            if (meta.Template.FieldType == templateType)
            {
                TraceManager.Trace(Unit, $"        TemplateFill(Module) successfully filled {meta.Template.FieldType.Name} with {templateType.Name}");
                meta.Template.Field.SetValue(m, template);
            }
            else
            {
                TraceManager.Trace(Unit,
                    $"        TemplateFill(Module) {templateType.Name} failed to fill {meta.Template.FieldType.Name} dependency");
            }
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
        public void Restart()
        {
            if (IsDisposed) return;
            if (!IsDynamic)
            {
                Reset();
                return;
            }
            RemoveAll();
            IsStarted = false;
            
            OnModuleAdded = null;
            OnModuleRemoved = null;
            OnTryToAddAlreadyExist = null;
            
            TraceManager.Trace(Unit, $"ModulesController Restarted");
            OnModulesRestarted?.Invoke();
            Start();
        }
        
        /// <summary>
        /// Just reset the modules, no reassembly
        /// </summary>
        public void Reset()
        {
            if (!IsStarted) return;
            if (IsDisposed) return;
            foreach (var module in _modules.Values)
                module.Reset();
            
            OnModuleAdded = null;
            OnModuleRemoved = null;
            OnTryToAddAlreadyExist = null;
            
            TraceManager.Trace(Unit, $"ModulesController Reseted");
            OnModulesReset?.Invoke();
            foreach (var module in _modules.Values)
                module.Start();
        }
        
        public void Dispose()
        {
            if (IsDisposed) return;
            TraceManager.Trace(Unit, $"ModulesController Disposed");
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
}