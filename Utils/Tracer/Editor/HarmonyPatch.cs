#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

using HarmonyLib;

using UnityEditor;
using UnityEngine;

namespace Sackrany.Utils.Tracer
{
    [InitializeOnLoad]
    public static class HarmonyTracer
    {
        const string HarmonyId = "sackrany.tracer";

        static Harmony _harmony;

        static readonly Dictionary<string, ParamInfo[]>             _paramInfos      = new();
        // _typeNames удалён — имя типа больше не используется в логе
        static readonly Dictionary<Type, Func<object, ITraceable>>  _resolvers        = new();
        static readonly Dictionary<Type, Func<object, ITraceable>>  _reflectionCache  = new();
        static readonly Dictionary<string, (string msg, int count)> _callCounters     = new();

        static readonly HashSet<string> _ignoredMethods = new()
        {
            "Equals", "GetHashCode", "ToString", "Finalize",
            "MemberwiseClone", "GetType", "ReferenceEquals"
        };

        static HarmonyTracer()
        {
            PatchAll();
            AssemblyReloadEvents.beforeAssemblyReload += UnpatchAll;
        }

        public static void RegisterResolver<T>(Func<T, ITraceable> resolver)
            => _resolvers[typeof(T)] = instance => resolver((T)instance);

        // ── Patch lifecycle ────────────────────────────────────────────────────

        static void PatchAll()
        {
            _harmony = new Harmony(HarmonyId);

            // Префикс с __args безопасен для out/ref — крашился только постфикс с __args.
            // Постфикс читает __result (не __args) — тоже безопасно.
            var prefix  = typeof(HarmonyTracer).GetMethod(nameof(OnEnterWithArgs), BindingFlags.Static | BindingFlags.NonPublic);
            var postfix = typeof(HarmonyTracer).GetMethod(nameof(OnExit),          BindingFlags.Static | BindingFlags.NonPublic);

            foreach (var type in GetTracedTypes())
            foreach (var method in GetPatchableMethods(type))
            {
                var key = MethodKey(method);
                _paramInfos[key] = BuildParamInfos(method);
                try
                {
                    _harmony.Patch(method,
                        prefix:  new HarmonyMethod(prefix),
                        postfix: new HarmonyMethod(postfix));
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogWarning($"[HarmonyTracer] Could not patch {type.Name}.{method.Name}: {e.Message}");
                }
            }
        }

        static void UnpatchAll()
        {
            _harmony?.UnpatchAll(HarmonyId);
            _paramInfos.Clear();
            _reflectionCache.Clear();
            _callCounters.Clear();
        }

        static IEnumerable<Type> GetTracedTypes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try   { types = assembly.GetTypes(); }
                catch { continue; }
                foreach (var t in types)
                    if (t.IsDefined(typeof(TraceAllAttribute), false))
                        yield return t;
            }
        }

        static IEnumerable<MethodInfo> GetPatchableMethods(Type type)
        {
            foreach (var m in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (m.IsSpecialName)                  continue;
                if (m.IsAbstract)                     continue;
                if (m.GetMethodBody() == null)        continue;
                if (_ignoredMethods.Contains(m.Name)) continue;
                yield return m;
            }
        }

        readonly struct ParamInfo
        {
            public readonly string Name;
            public readonly bool   IsOut;
            public ParamInfo(string name, bool isOut) { Name = name; IsOut = isOut; }
        }

        static string MethodKey(MethodBase m) => $"{m.DeclaringType!.FullName}.{m.Name}";

        static bool HasOutOrRef(MethodInfo m)
        {
            foreach (var p in m.GetParameters())
                if (p.IsOut || p.ParameterType.IsByRef) return true;
            return false;
        }

        static ParamInfo[] BuildParamInfos(MethodInfo method)
        {
            var parms = method.GetParameters();
            var result = new ParamInfo[parms.Length];
            for (int i = 0; i < parms.Length; i++)
                result[i] = new ParamInfo(parms[i].Name, parms[i].IsOut || parms[i].ParameterType.IsByRef);
            return result;
        }

        static string FormatParamInfos(ParamInfo[] infos, object[] args)
        {
            if (infos == null || infos.Length == 0) return string.Empty;
            var sb = new StringBuilder();
            for (int i = 0; i < infos.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(infos[i].Name).Append(": ");
                // out-параметры на входе uninitialized — показываем стрелку "будет заполнено"
                if (infos[i].IsOut) sb.Append("→");
                else if (args != null && i < args.Length) sb.Append(SafeToString(args[i]));
                else sb.Append("?");
            }
            return sb.ToString();
        }

        // ── Thread-local state ─────────────────────────────────────────────────

        [ThreadStatic] static bool              _insideHook;
        [ThreadStatic] static int               _depth;
        [ThreadStatic] static Stack<FrameState> _callStack;

        static Stack<FrameState> CallStack => _callStack ??= new Stack<FrameState>();

        struct FrameState
        {
            public long       StartTicks;
            public bool       Traced;
            public ITraceable Traceable;
            public string     MethodName;
            public string     Key;
            public string     Indent;
            public object[]   ArgsRef;   // ссылка на массив Harmony — после выполнения метода содержит out/ref значения
        }

        // ── Hooks ──────────────────────────────────────────────────────────────

        static void OnEnterWithArgs(object __instance, MethodBase __originalMethod, object[] __args)
        {
            OnEnterCore(__instance, __originalMethod, __args);
        }

        static void OnEnter(object __instance, MethodBase __originalMethod)
        {
            OnEnterCore(__instance, __originalMethod, null);
        }

        static void OnEnterCore(object __instance, MethodBase __originalMethod, object[] args)
        {
            if (_insideHook) return;
            _insideHook = true;
            try
            {
                var traceable = ResolveTraceable(__instance);
                var key       = MethodKey(__originalMethod);
                var indent    = new string(' ', _depth * 2);

                if (traceable == null || !traceable.IsTracing())
                {
                    CallStack.Push(new FrameState { Traced = false });
                    _depth++;
                    return;
                }

                var methodName = __originalMethod.Name;
                var infos      = _paramInfos.TryGetValue(key, out var pi) ? pi : null;
                var paramStr   = FormatParamInfos(infos, args);
                var msg        = $"{indent}→ {methodName}({paramStr})";

                if (_callCounters.TryGetValue(key, out var counter) && counter.msg == msg)
                    _callCounters[key] = (msg, counter.count + 1);
                else
                {
                    FlushCounter(traceable, key, indent);
                    _callCounters[key] = (msg, 1);
                }
                TraceManager.Trace(traceable, msg); // → всегда

                // StartTicks обновляется при каждом вызове — иначе таймер [x2] врёт
                CallStack.Push(new FrameState
                {
                    StartTicks = Stopwatch.GetTimestamp(),
                    Traced     = true,
                    Traceable  = traceable,
                    MethodName = methodName,
                    Key        = key,
                    Indent     = indent,
                    ArgsRef    = args,
                });
                _depth++;
            }
            finally { _insideHook = false; }
        }

        static void OnExit(object __instance, MethodBase __originalMethod, object __result)
        {
            if (_insideHook) return;
            _insideHook = true;
            try
            {
                _depth--;
                if (_depth < 0) _depth = 0;

                var frame = CallStack.Count > 0 ? CallStack.Pop() : default;
                if (!frame.Traced) return;

                var traceable = frame.Traceable ?? ResolveTraceable(__instance);
                if (traceable == null || !traceable.IsTracing()) return;

                var timeStr = frame.StartTicks > 0
                    ? $"  [{(Stopwatch.GetTimestamp() - frame.StartTicks) * 1000.0 / Stopwatch.Frequency:F2}ms]"
                    : string.Empty;

                var countStr = string.Empty;
                if (frame.Key != null && _callCounters.TryGetValue(frame.Key, out var counter) && counter.count > 1)
                {
                    countStr = $"  [x{counter.count}]";
                    _callCounters.Remove(frame.Key);
                }

                var resultStr = SafeToString(__result);
                var outStr    = BuildOutStr(frame.Key, frame.ArgsRef);
                var main      = $"{frame.Indent}← {frame.MethodName} = {resultStr}";
                var meta      = $"{outStr}{timeStr}{countStr}";
                var line      = meta.Length > 0 ? PadToColumn(main, 64) + meta : main;
                TraceManager.Trace(traceable, line);
            }
            finally { _insideHook = false; }
        }



        static string SafeToString(object obj)
        {
            if (obj == null)                   return "null";
            if (obj is bool b)                 return b ? "true" : "false";
            if (obj is string s)               return $"\"{s}\"";
            if (obj is UnityEngine.Object u)   return $"{u.GetType().Name}(\"{u.name}\")";
            if (obj is Type t)                 return t.Name;
            try   { return obj.ToString(); }
            catch { return obj.GetType().Name; }
        }

        // ── Formatting ─────────────────────────────────────────────────────────

        static string BuildOutStr(string key, object[] args)
        {
            if (args == null || args.Length == 0) return string.Empty;
            if (!_paramInfos.TryGetValue(key ?? string.Empty, out var infos)) return string.Empty;

            var sb  = new StringBuilder();
            bool any = false;
            for (int i = 0; i < infos.Length && i < args.Length; i++)
            {
                if (!infos[i].IsOut) continue;
                if (args[i] == null) continue; // null = не было записано Harmony, не показываем
                if (!any) { sb.Append("  [out "); any = true; }
                else sb.Append(", ");
                sb.Append(infos[i].Name).Append(": ").Append(SafeToString(args[i]));
            }
            if (any) sb.Append(']');
            return sb.ToString();
        }

        const int MetaColumn = 64; // колонка с которой начинаются [] мета-данные

        static string PadToColumn(string s, int column)
        {
            if (s.Length >= column) return s + "  ";
            return s + new string(' ', column - s.Length);
        }

        static void FlushCounter(ITraceable traceable, string key, string indent)
        {
            if (!_callCounters.TryGetValue(key, out var prev) || prev.count <= 1) return;
            TraceManager.Trace(traceable, $"{indent}  ... repeated x{prev.count}");
            _callCounters.Remove(key);
        }

        // ── ITraceable resolution ──────────────────────────────────────────────

        static ITraceable ResolveTraceable(object instance)
        {
            if (instance == null) return null;
            var type = instance.GetType();
            if (_resolvers.TryGetValue(type, out var resolver)) return resolver(instance);
            if (instance is ITraceable traceable)               return traceable;
            if (instance is ITraceableProvider provider)        return provider.GetTraceable();
            return FindTraceableByReflection(type, instance);
        }

        static ITraceable FindTraceableByReflection(Type type, object instance)
        {
            if (_reflectionCache.TryGetValue(type, out var cached))
                return cached(instance);

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var field in type.GetFields(flags))
            {
                if (!typeof(ITraceable).IsAssignableFrom(field.FieldType)) continue;
                Func<object, ITraceable> getter = o => (ITraceable)field.GetValue(o);
                _reflectionCache[type] = getter;
                return getter(instance);
            }
            foreach (var prop in type.GetProperties(flags))
            {
                if (!typeof(ITraceable).IsAssignableFrom(prop.PropertyType) || !prop.CanRead) continue;
                Func<object, ITraceable> getter = o => (ITraceable)prop.GetValue(o);
                _reflectionCache[type] = getter;
                return getter(instance);
            }

            _reflectionCache[type] = _ => null;
            return null;
        }
    }
}
#endif