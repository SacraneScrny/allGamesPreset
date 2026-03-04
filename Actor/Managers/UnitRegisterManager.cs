using System;
using System.Collections.Generic;
using System.Linq;

using Sackrany.Actor.Traits.Tags;
using Sackrany.Actor.Unit;
using Sackrany.Utils;
using Sackrany.Utils.CacheRegistry;

namespace Sackrany.Actor.Managers
{
    public class UnitRegisterManager : AManager<UnitRegisterManager>
    {
        readonly Dictionary<TeamInfo, Dictionary<uint, Unit.Unit>> _cachedTeams = new();
        readonly Dictionary<uint, Unit.Unit> _cachedUnits = new();
        readonly Dictionary<UnitArchetype, Dictionary<uint, Unit.Unit>> _cachedArchetypes = new();
        readonly Dictionary<int, Dictionary<uint, Unit.Unit>> _cachedTags = new();
        readonly List<Unit.Unit> _cachedArray = new();

        public static IReadOnlyList<Unit.Unit> RegisteredUnits => Instance._cachedArray;

        public static bool RegisterUnit(Unit.Unit unit)
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
            Instance._cachedArray.Add(unit);
            Instance.OnUnitRegistered?.Invoke(unit);
            unit.OnStartWorking += Instance.HandleUnitStarted;
            unit.Tag.OnTagAdded += (id) => Instance.OnUnitTagAdded(unit, id);
            unit.Tag.OnTagRemoved += (id) => Instance.OnUnitTagRemoved(unit, id);
            return true;
        }
        static bool RegisterTeam(Unit.Unit unit)
        {
            if (!Instance._cachedTeams.TryGetValue(unit.Team, out var team))
            {
                team = new();
                Instance._cachedTeams.Add(unit.Team, team);
            }
            return team.TryAdd(unit.Hash, unit);
        }
        static void RegisterTags(Unit.Unit unit)
        {
            foreach (var id in unit.Tag.GetIds())
                AddToTagIndex(unit, id);
        }

        static void AddToTagIndex(Unit.Unit unit, int tagId)
        {
            if (!Instance._cachedTags.TryGetValue(tagId, out var bucket))
            {
                bucket = new();
                Instance._cachedTags.Add(tagId, bucket);
            }
            bucket.TryAdd(unit.Hash, unit);
        }
        static void RemoveFromTagIndex(Unit.Unit unit, int tagId)
        {
            if (Instance._cachedTags.TryGetValue(tagId, out var bucket))
                bucket.Remove(unit.Hash);
        }

        void OnUnitTagAdded(Unit.Unit unit, int tagId) => AddToTagIndex(unit, tagId);
        void OnUnitTagRemoved(Unit.Unit unit, int tagId) => RemoveFromTagIndex(unit, tagId);

        public static bool UnregisterUnit(Unit.Unit unit)
        {
            if (!Instance._cachedUnits.ContainsKey(unit.Hash)) return false;

            UnregisterTeam(unit);
            UnregisterTags(unit);

            if (Instance._cachedArchetypes.TryGetValue(unit.Archetype, out var archetypes))
                archetypes.Remove(unit.Hash);

            Instance._cachedUnits.Remove(unit.Hash);
            Instance._cachedArray.Remove(unit);
            unit.OnStartWorking -= Instance.HandleUnitStarted;
            unit.Tag.OnTagAdded -= (id) => Instance.OnUnitTagAdded(unit, id);
            unit.Tag.OnTagRemoved -= (id) => Instance.OnUnitTagRemoved(unit, id);
            Instance.OnUnitUnregistered?.Invoke(unit);
            return true;
        }
        static bool UnregisterTeam(Unit.Unit unit)
        {
            if (!Instance._cachedTeams.TryGetValue(unit.Team, out var team)) return false;
            return team.Remove(unit.Hash);
        }
        static void UnregisterTags(Unit.Unit unit)
        {
            foreach (var id in unit.Tag.GetIds())
                RemoveFromTagIndex(unit, id);
        }

        public static bool HasUnits(Func<Unit.Unit, bool> cond)
        {
            foreach (var unit in Instance._cachedArray)
                if (cond(unit)) return true;
            return false;
        }
        public static bool HasUnitsWithTag<T>() where T : ITag
            => Instance._cachedTags.TryGetValue(TypeRegistry<ITag>.Id<T>.Value, out var b) && b.Count > 0;

        #region GET
        public static Unit.Unit GetUnit(Func<Unit.Unit, bool> cond)
        {
            for (var i = 0; i < Instance._cachedArray.Count; i++)
            {
                var unit = Instance._cachedArray[i];
                if (cond(unit)) return unit;
            }
            return null;
        }
        public static Unit.Unit GetUnitWithTag<T>() where T : ITag
        {
            int id = TypeRegistry<ITag>.Id<T>.Value;
            if (!Instance._cachedTags.TryGetValue(id, out var bucket)) return null;
            foreach (var kvp in bucket)
                if (kvp.Value.IsActive) return kvp.Value;
            return null;
        }
        public static Unit.Unit GetUnitWithTag<T>(Func<Unit.Unit, bool> cond) where T : ITag
        {
            int id = TypeRegistry<ITag>.Id<T>.Value;
            if (!Instance._cachedTags.TryGetValue(id, out var bucket)) return null;
            foreach (var kvp in bucket)
                if (kvp.Value.IsActive && cond(kvp.Value)) return kvp.Value;
            return null;
        }

        public static bool TryGetUnit(Func<Unit.Unit, bool> cond, out Unit.Unit value)
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
        public static bool TryGetUnit(TeamInfo team, Func<Unit.Unit, bool> cond, out Unit.Unit value)
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
        public static bool TryGetUnit(TeamInfo team, out Unit.Unit value)
        {
            if (!Instance._cachedTeams.TryGetValue(team, out var teams))
            {
                value = null;
                return false;
            }
            value = teams.First().Value;
            return true;
        }
        public static bool TryGetUnitWithTag<T>(out Unit.Unit value) where T : ITag
        {
            value = GetUnitWithTag<T>();
            return value != null;
        }
        public static bool TryGetUnitWithTag<T>(Func<Unit.Unit, bool> cond, out Unit.Unit value) where T : ITag
        {
            value = GetUnitWithTag<T>(cond);
            return value != null;
        }

        public static bool TryGetUnits(Func<Unit.Unit, bool> cond, out Unit.Unit[] value)
        {
            value = GetAllUnits(cond).ToArray();
            return value.Length > 0;
        }
        public static bool TryGetUnits(TeamInfo team, Func<Unit.Unit, bool> cond, out Unit.Unit[] value)
        {
            value = GetAllUnits(team, cond).ToArray();
            return value.Length > 0;
        }
        public static bool TryGetUnits(TeamInfo team, out Unit.Unit[] value)
        {
            value = GetAllUnits(team).ToArray();
            return value.Length > 0;
        }

        public static bool TryGetUnits(Func<Unit.Unit, bool> cond, out List<Unit.Unit> value)
        {
            value = GetAllUnits(cond).ToList();
            return value.Count > 0;
        }
        public static bool TryGetUnits(TeamInfo team, Func<Unit.Unit, bool> cond, out List<Unit.Unit> value)
        {
            value = GetAllUnits(team, cond).ToList();
            return value.Count > 0;
        }
        public static bool TryGetUnits(TeamInfo team, out List<Unit.Unit> value)
        {
            value = GetAllUnits(team).ToList();
            return value.Count > 0;
        }

        public static bool TryGetUnitsWithTag<T>(out Unit.Unit[] value) where T : ITag
        {
            value = GetAllUnitsWithTag<T>().ToArray();
            return value.Length > 0;
        }
        public static bool TryGetUnitsWithTag<T>(Func<Unit.Unit, bool> cond, out Unit.Unit[] value) where T : ITag
        {
            value = GetAllUnitsWithTag<T>(cond).ToArray();
            return value.Length > 0;
        }

        public static IReadOnlyList<Unit.Unit> GetAllUnits() => Instance._cachedArray;
        public static IReadOnlyList<Unit.Unit> GetAllUnits(TeamInfo team) => 
            !Instance._cachedTeams.TryGetValue(team, out var teams)
                ? Array.Empty<Unit.Unit>()
                : teams.Select(x => x.Value).ToList().AsReadOnly();
        public static IEnumerable<Unit.Unit> GetAllUnits(Func<Unit.Unit, bool> cond) => Instance._cachedArray.Where(cond);
        public static IEnumerable<Unit.Unit> GetAllUnits(TeamInfo team, Func<Unit.Unit, bool> cond) => 
            !Instance._cachedTeams.TryGetValue(team, out var teams)
                ? Array.Empty<Unit.Unit>()
                : teams.Select(x => x.Value).Where(cond);
        public static IEnumerable<Unit.Unit> GetAllUnits(UnitArchetype archetype) => 
            Instance._cachedArchetypes.TryGetValue(archetype, out var archetypes)
                ? archetypes.Select(x => x.Value)
                : Array.Empty<Unit.Unit>();

        public static IEnumerable<Unit.Unit> GetAllUnitsWithTag<T>() where T : ITag
        {
            int id = TypeRegistry<ITag>.Id<T>.Value;
            if (!Instance._cachedTags.TryGetValue(id, out var bucket)) return Array.Empty<Unit.Unit>();
            return bucket.Values.Where(u => u.IsActive);
        }
        public static IEnumerable<Unit.Unit> GetAllUnitsWithTag<T>(Func<Unit.Unit, bool> cond) where T : ITag
        {
            int id = TypeRegistry<ITag>.Id<T>.Value;
            if (!Instance._cachedTags.TryGetValue(id, out var bucket)) return Array.Empty<Unit.Unit>();
            return bucket.Values.Where(u => u.IsActive && cond(u));
        }
        #endregion

        public event Action<Unit.Unit> OnUnitRegistered;
        public event Action<Unit.Unit> OnUnitUnregistered;
        public event Action<Unit.Unit> OnUnitStarted;

        void HandleUnitStarted(Unit.Unit unit) => OnUnitStarted?.Invoke(unit);
    }
}