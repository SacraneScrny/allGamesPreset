using System;
using System.Collections.Generic;
using System.Linq;

using Sackrany.Actor.Traits.Tags;
using Sackrany.Actor.UnitMono;
using Sackrany.Utils;
using Sackrany.Utils.CacheRegistry;

namespace Sackrany.Actor.Managers
{
    public class UnitRegisterManager : AManager<UnitRegisterManager>
    {
        readonly Dictionary<TeamInfo, Dictionary<uint, Unit>> _cachedTeams = new();
        readonly Dictionary<uint, Unit> _cachedUnits = new();
        readonly Dictionary<UnitArchetype, Dictionary<uint, Unit>> _cachedArchetypes = new();
        readonly Dictionary<int, Dictionary<uint, Unit>> _cachedTags = new();
        readonly List<Unit> _cachedArray = new();
        
        readonly Dictionary<uint, int> _hashToIndex = new();
        readonly Dictionary<uint, UnitHandlers> _handlers = new();
        readonly struct UnitHandlers
        {
            public readonly Action<int> OnTagAdded;
            public readonly Action<int> OnTagRemoved;
            public UnitHandlers(Action<int> onTagAdded, Action<int> onTagRemoved)
            {
                OnTagAdded   = onTagAdded;
                OnTagRemoved = onTagRemoved;
            }
        }
        
        public static IReadOnlyList<Unit> RegisteredUnits => Instance._cachedArray;

        public static bool RegisterUnit(Unit unit)
        {
            if (Instance._cachedUnits.ContainsKey(unit.Hash)) return false;

            if (!Instance._cachedArchetypes.TryGetValue(unit.Archetype, out var archetypes))
            {
                archetypes = new();
                Instance._cachedArchetypes.Add(unit.Archetype, archetypes);
            }

            RegisterTeam(unit);
            RegisterTags(unit);
            archetypes.TryAdd(unit.Hash, unit);
            Instance._cachedUnits.Add(unit.Hash, unit);
            
            Instance._hashToIndex[unit.Hash] = Instance._cachedArray.Count;
            Instance._cachedArray.Add(unit);

            var handlers = new UnitHandlers(
                onTagAdded:   id => Instance.OnUnitTagAdded(unit, id),
                onTagRemoved: id => Instance.OnUnitTagRemoved(unit, id)
            );
            Instance._handlers[unit.Hash] = handlers;
            unit.OnStartWorking    += Instance.HandleUnitStarted;
            unit.Tag.OnTagAdded    += handlers.OnTagAdded;
            unit.Tag.OnTagRemoved  += handlers.OnTagRemoved;

            Instance.OnUnitRegistered?.Invoke(unit);
            return true;
        }
        static bool RegisterTeam(Unit unit)
        {
            if (!Instance._cachedTeams.TryGetValue(unit.Team, out var team))
            {
                team = new();
                Instance._cachedTeams.Add(unit.Team, team);
            }
            return team.TryAdd(unit.Hash, unit);
        }
        static void RegisterTags(Unit unit)
        {
            foreach (var id in unit.Tag.GetIds())
                AddToTagIndex(unit, id);
        }

        static void AddToTagIndex(Unit unit, int tagId)
        {
            if (!Instance._cachedTags.TryGetValue(tagId, out var bucket))
            {
                bucket = new();
                Instance._cachedTags.Add(tagId, bucket);
            }
            bucket.TryAdd(unit.Hash, unit);
        }
        static void RemoveFromTagIndex(Unit unit, int tagId)
        {
            if (Instance._cachedTags.TryGetValue(tagId, out var bucket))
                bucket.Remove(unit.Hash);
        }

        void OnUnitTagAdded(Unit unit, int tagId) => AddToTagIndex(unit, tagId);
        void OnUnitTagRemoved(Unit unit, int tagId) => RemoveFromTagIndex(unit, tagId);

        public static bool UnregisterUnit(Unit unit)
        {
            if (!Instance._cachedUnits.ContainsKey(unit.Hash)) return false;

            UnregisterTeam(unit);
            UnregisterTags(unit);
            
            if (Instance._cachedArchetypes.TryGetValue(unit.Archetype, out var archetypes))
                archetypes.Remove(unit.Hash);
            Instance._cachedUnits.Remove(unit.Hash);

            int idx  = Instance._hashToIndex[unit.Hash];
            int last = Instance._cachedArray.Count - 1;
            if (idx != last)
            {
                var swapped = Instance._cachedArray[last];
                Instance._cachedArray[idx]       = swapped;
                Instance._hashToIndex[swapped.Hash] = idx;
            }
            Instance._cachedArray.RemoveAt(last);
            Instance._hashToIndex.Remove(unit.Hash);

            if (Instance._handlers.TryGetValue(unit.Hash, out var handlers))
            {
                unit.OnStartWorking   -= Instance.HandleUnitStarted;
                unit.Tag.OnTagAdded   -= handlers.OnTagAdded;
                unit.Tag.OnTagRemoved -= handlers.OnTagRemoved;
                Instance._handlers.Remove(unit.Hash);
            }

            Instance.OnUnitUnregistered?.Invoke(unit);
            return true;
        }
        static bool UnregisterTeam(Unit unit)
        {
            if (!Instance._cachedTeams.TryGetValue(unit.Team, out var team)) return false;
            return team.Remove(unit.Hash);
        }
        static void UnregisterTags(Unit unit)
        {
            foreach (var id in unit.Tag.GetIds())
                RemoveFromTagIndex(unit, id);
        }

        public static bool HasUnits(Func<Unit, bool> cond)
        {
            foreach (var unit in Instance._cachedArray)
                if (cond(unit)) return true;
            return false;
        }
        public static bool HasUnitsWithTag<T>() where T : ITag
            => Instance._cachedTags.TryGetValue(TypeRegistry<ITag>.Id<T>.Value, out var b) && b.Count > 0;

        #region GET
        public static Unit GetUnit(Func<Unit, bool> cond)
        {
            for (var i = 0; i < Instance._cachedArray.Count; i++)
            {
                var unit = Instance._cachedArray[i];
                if (cond(unit)) return unit;
            }
            return null;
        }
        public static Unit GetUnitWithTag<T>() where T : ITag
        {
            int id = TypeRegistry<ITag>.Id<T>.Value;
            if (!Instance._cachedTags.TryGetValue(id, out var bucket)) return null;
            foreach (var kvp in bucket)
                if (kvp.Value.IsActive) return kvp.Value;
            return null;
        }
        public static Unit GetUnitWithTag<T>(Func<Unit, bool> cond) where T : ITag
        {
            int id = TypeRegistry<ITag>.Id<T>.Value;
            if (!Instance._cachedTags.TryGetValue(id, out var bucket)) return null;
            foreach (var kvp in bucket)
                if (kvp.Value.IsActive && cond(kvp.Value)) return kvp.Value;
            return null;
        }

        public static bool TryGetUnit(Func<Unit, bool> cond, out Unit value)
        {
            foreach (var unit in Instance._cachedArray)
                if (cond(unit))
                {
                    value = unit;
                    return true;
                }
            value = null;
            return false;
        }
        public static bool TryGetUnit(TeamInfo team, Func<Unit, bool> cond, out Unit value)
        {
            if (!Instance._cachedTeams.TryGetValue(team, out var teams))
            {
                value = null;
                return false;
            }
            foreach (var unit in teams)
                if (cond(unit.Value))
                {
                    value = unit.Value;
                    return true;
                }
            value = null;
            return false;
        }
        public static bool TryGetUnit(TeamInfo team, out Unit value)
        {
            if (!Instance._cachedTeams.TryGetValue(team, out var teams))
            {
                value = null;
                return false;
            }
            value = teams.First().Value;
            return true;
        }
        public static bool TryGetUnitWithTag<T>(out Unit value) where T : ITag
        {
            value = GetUnitWithTag<T>();
            return value != null;
        }
        public static bool TryGetUnitWithTag<T>(Func<Unit, bool> cond, out Unit value) where T : ITag
        {
            value = GetUnitWithTag<T>(cond);
            return value != null;
        }

        public static bool TryGetUnits(Func<Unit, bool> cond, out Unit[] value)
        {
            value = GetAllUnits(cond).ToArray();
            return value.Length > 0;
        }
        public static bool TryGetUnits(TeamInfo team, Func<Unit, bool> cond, out Unit[] value)
        {
            value = GetAllUnits(team, cond).ToArray();
            return value.Length > 0;
        }
        public static bool TryGetUnits(TeamInfo team, out Unit[] value)
        {
            value = GetAllUnits(team).ToArray();
            return value.Length > 0;
        }

        public static bool TryGetUnits(Func<Unit, bool> cond, out List<Unit> value)
        {
            value = GetAllUnits(cond).ToList();
            return value.Count > 0;
        }
        public static bool TryGetUnits(TeamInfo team, Func<Unit, bool> cond, out List<Unit> value)
        {
            value = GetAllUnits(team, cond).ToList();
            return value.Count > 0;
        }
        public static bool TryGetUnits(TeamInfo team, out List<Unit> value)
        {
            value = GetAllUnits(team).ToList();
            return value.Count > 0;
        }

        public static bool TryGetUnitsWithTag<T>(out Unit[] value) where T : ITag
        {
            value = GetAllUnitsWithTag<T>().ToArray();
            return value.Length > 0;
        }
        public static bool TryGetUnitsWithTag<T>(Func<Unit, bool> cond, out Unit[] value) where T : ITag
        {
            value = GetAllUnitsWithTag<T>(cond).ToArray();
            return value.Length > 0;
        }

        public static IReadOnlyList<Unit> GetAllUnits() => Instance._cachedArray;
        public static IReadOnlyList<Unit> GetAllUnits(TeamInfo team) => 
            !Instance._cachedTeams.TryGetValue(team, out var teams)
                ? Array.Empty<Unit>()
                : teams.Select(x => x.Value).ToList().AsReadOnly();
        public static IEnumerable<Unit> GetAllUnits(Func<Unit, bool> cond) => Instance._cachedArray.Where(cond);
        public static IEnumerable<Unit> GetAllUnits(TeamInfo team, Func<Unit, bool> cond) => 
            !Instance._cachedTeams.TryGetValue(team, out var teams)
                ? Array.Empty<Unit>()
                : teams.Select(x => x.Value).Where(cond);
        public static IEnumerable<Unit> GetAllUnits(UnitArchetype archetype) => 
            Instance._cachedArchetypes.TryGetValue(archetype, out var archetypes)
                ? archetypes.Select(x => x.Value)
                : Array.Empty<Unit>();

        public static IEnumerable<Unit> GetAllUnitsWithTag<T>() where T : ITag
        {
            int id = TypeRegistry<ITag>.Id<T>.Value;
            if (!Instance._cachedTags.TryGetValue(id, out var bucket)) return Array.Empty<Unit>();
            return bucket.Values.Where(u => u.IsActive);
        }
        public static IEnumerable<Unit> GetAllUnitsWithTag<T>(Func<Unit, bool> cond) where T : ITag
        {
            int id = TypeRegistry<ITag>.Id<T>.Value;
            if (!Instance._cachedTags.TryGetValue(id, out var bucket)) return Array.Empty<Unit>();
            return bucket.Values.Where(u => u.IsActive && cond(u));
        }
        #endregion

        public event Action<Unit> OnUnitRegistered;
        public event Action<Unit> OnUnitUnregistered;
        public event Action<Unit> OnUnitStarted;

        void HandleUnitStarted(Unit unit) => OnUnitStarted?.Invoke(unit);
    }
}