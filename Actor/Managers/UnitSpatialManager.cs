using System;
using System.Collections.Generic;

using Sackrany.Actor.UnitMono;
using Sackrany.Utils;

using UnityEngine;

namespace Sackrany.Actor.Managers
{
    public class UnitSpatialManager : AManager<UnitSpatialManager>
    {
        [SerializeField] float CellSize = 10f;

        readonly Dictionary<CellKey, List<Unit>> _cells = new();
        readonly Dictionary<uint, CellKey> _unitCell = new();
        readonly Dictionary<uint, Unit> _units = new();
        
        readonly List<Unit> _unitList = new();
        readonly Dictionary<uint, int> _unitListIndex = new();

        float _invCellSize;

        private protected override void OnInitialize()
        {
            _invCellSize = 1f / CellSize;
        }
        private protected override void OnManagerAwake()
        {
            var registered = UnitRegisterManager.RegisteredUnits;
            for (int i = 0; i < registered.Count; i++)
                RegisterInternal(registered[i]);

            UnitRegisterManager.Instance.OnUnitRegistered   += RegisterInternal;
            UnitRegisterManager.Instance.OnUnitUnregistered += UnregisterInternal;
        }
        private protected override void OnManagerDestroy()
        {
            if (UnitRegisterManager.Instance == null) return;
            UnitRegisterManager.Instance.OnUnitRegistered   -= RegisterInternal;
            UnitRegisterManager.Instance.OnUnitUnregistered -= UnregisterInternal;
        }

        void RegisterInternal(Unit unit)
        {
            var key = GetCell(unit.transform.position);
            AddToCell(unit, key);
            _unitCell[unit.Hash] = key;
            _units[unit.Hash] = unit;
            _unitListIndex[unit.Hash] = _unitList.Count;
            _unitList.Add(unit);
        }
        void UnregisterInternal(Unit unit)
        {
            if (!_unitCell.TryGetValue(unit.Hash, out var key)) return;
            RemoveFromCell(unit, key);
            _unitCell.Remove(unit.Hash);
            _units.Remove(unit.Hash);
            
            int idx  = _unitListIndex[unit.Hash];
            int last = _unitList.Count - 1;
            if (idx != last)
            {
                var swapped = _unitList[last];
                _unitList[idx] = swapped;
                _unitListIndex[swapped.Hash] = idx;
            }
            _unitList.RemoveAt(last);
            _unitListIndex.Remove(unit.Hash);
        }
        void UpdatePositionInternal(Unit unit)
        {
            var newKey = GetCell(unit.transform.position);
            if (!_unitCell.TryGetValue(unit.Hash, out var oldKey)) return;
            if (oldKey == newKey) return;
            RemoveFromCell(unit, oldKey);
            AddToCell(unit, newKey);
            _unitCell[unit.Hash] = newKey;
        }
        
        void Update()
        {
            for (int i = 0; i < _unitList.Count; i++)
                UpdatePositionInternal(_unitList[i]);
        }
        
        public static void GetInRadius(Vector3 center, float radius, List<Unit> results)
            => Instance.GetInRadiusInternal(center, radius, results, null);
        public static void GetInRadius(Vector3 center, float radius, List<Unit> results, Func<Unit, bool> cond)
            => Instance.GetInRadiusInternal(center, radius, results, cond);
        public static bool TryGetNearest(Vector3 center, float radius, out Unit result)
            => Instance.TryGetNearestInternal(center, radius, null, out result);
        public static bool TryGetNearest(Vector3 center, float radius, Func<Unit, bool> cond, out Unit result)
            => Instance.TryGetNearestInternal(center, radius, cond, out result);
        public static bool HasAnyInRadius(Vector3 center, float radius)
            => Instance.HasAnyInRadiusInternal(center, radius, null);
        public static bool HasAnyInRadius(Vector3 center, float radius, Func<Unit, bool> cond)
            => Instance.HasAnyInRadiusInternal(center, radius, cond);

        void GetInRadiusInternal(Vector3 center, float radius, List<Unit> results, Func<Unit, bool> cond)
        {
            results.Clear();
            float radiusSq = radius * radius;
            int minX = Mathf.FloorToInt((center.x - radius) * _invCellSize);
            int maxX = Mathf.FloorToInt((center.x + radius) * _invCellSize);
            int minZ = Mathf.FloorToInt((center.z - radius) * _invCellSize);
            int maxZ = Mathf.FloorToInt((center.z + radius) * _invCellSize);

            for (int x = minX; x <= maxX; x++)
                for (int z = minZ; z <= maxZ; z++)
                {
                    if (!_cells.TryGetValue(new CellKey(x, z), out var cell)) continue;
                    for (int i = 0; i < cell.Count; i++)
                    {
                        var unit = cell[i];
                        if (unit == null || !unit.IsActive) continue;
                        if (Vector3.SqrMagnitude(unit.transform.position - center) > radiusSq) continue;
                        if (cond != null && !cond(unit)) continue;
                        results.Add(unit);
                    }
                }
        }
        bool TryGetNearestInternal(Vector3 center, float radius, Func<Unit, bool> cond, out Unit result)
        {
            Unit nearest = null;
            float bestSq = radius * radius;

            int minX = Mathf.FloorToInt((center.x - radius) * _invCellSize);
            int maxX = Mathf.FloorToInt((center.x + radius) * _invCellSize);
            int minZ = Mathf.FloorToInt((center.z - radius) * _invCellSize);
            int maxZ = Mathf.FloorToInt((center.z + radius) * _invCellSize);

            for (int x = minX; x <= maxX; x++)
                for (int z = minZ; z <= maxZ; z++)
                {
                    if (!_cells.TryGetValue(new CellKey(x, z), out var cell)) continue;
                    for (int i = 0; i < cell.Count; i++)
                    {
                        var unit = cell[i];
                        if (unit == null || !unit.IsActive) continue;
                        float sq = Vector3.SqrMagnitude(unit.transform.position - center);
                        if (sq > bestSq) continue;
                        if (cond != null && !cond(unit)) continue;
                        bestSq  = sq;
                        nearest = unit;
                    }
                }

            result = nearest;
            return result != null;
        }
        bool HasAnyInRadiusInternal(Vector3 center, float radius, Func<Unit, bool> cond)
        {
            float radiusSq = radius * radius;

            int minX = Mathf.FloorToInt((center.x - radius) * _invCellSize);
            int maxX = Mathf.FloorToInt((center.x + radius) * _invCellSize);
            int minZ = Mathf.FloorToInt((center.z - radius) * _invCellSize);
            int maxZ = Mathf.FloorToInt((center.z + radius) * _invCellSize);

            for (int x = minX; x <= maxX; x++)
                for (int z = minZ; z <= maxZ; z++)
                {
                    if (!_cells.TryGetValue(new CellKey(x, z), out var cell)) continue;
                    for (int i = 0; i < cell.Count; i++)
                    {
                        var unit = cell[i];
                        if (unit == null || !unit.IsActive) continue;
                        if (Vector3.SqrMagnitude(unit.transform.position - center) > radiusSq) continue;
                        if (cond != null && !cond(unit)) continue;
                        return true;
                    }
                }

            return false;
        }

        CellKey GetCell(Vector3 pos)
            => new CellKey(
                Mathf.FloorToInt(pos.x * _invCellSize),
                Mathf.FloorToInt(pos.z * _invCellSize)
            );

        void AddToCell(Unit unit, CellKey key)
        {
            if (!_cells.TryGetValue(key, out var list))
            {
                list = new List<Unit>(8);
                _cells[key] = list;
            }
            list.Add(unit);
        }
        void RemoveFromCell(Unit unit, CellKey key)
        {
            if (!_cells.TryGetValue(key, out var list)) return;
            int idx = list.IndexOf(unit);
            if (idx < 0) return;
            int last = list.Count - 1;
            if (idx != last)
                list[idx] = list[last];
            list.RemoveAt(last);
            if (list.Count == 0)
                _cells.Remove(key);
        }

        readonly struct CellKey : IEquatable<CellKey>
        {
            readonly int _x;
            readonly int _z;

            public CellKey(int x, int z) { _x = x; _z = z; }

            public bool Equals(CellKey other) => _x == other._x && _z == other._z;
            public override bool Equals(object obj) => obj is CellKey other && Equals(other);

            public override int GetHashCode()
            {
                uint h = 2166136261u;
                h = (h ^ unchecked((uint)_x)) * 16777619u;
                h = (h ^ unchecked((uint)_z)) * 16777619u;
                return unchecked((int)h);
            }

            public static bool operator ==(CellKey a, CellKey b) => a.Equals(b);
            public static bool operator !=(CellKey a, CellKey b) => !a.Equals(b);
        }
    }
}