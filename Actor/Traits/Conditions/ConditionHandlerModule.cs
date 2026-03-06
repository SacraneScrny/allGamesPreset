using System;
using System.Collections.Generic;
using Sackrany.Actor.Modules;
using Sackrany.Actor.Modules.Modules;
using Sackrany.Actor.Traits.Conditions.Static;
using Sackrany.Variables.ExpandedVariable.Abstracts;
using Sackrany.Variables.ExpandedVariable.Entities;

using UnityEngine;

namespace Sackrany.Actor.Traits.Conditions
{
    public class ConditionHandlerModule : Module
    {
        [Template] ConditionHandler _template;

        readonly Dictionary<int, int> _blocks = new();
        readonly Dictionary<int, ExpandedBool> _gates = new();

        protected override void OnStart()
        {
            if (_template.Default == null) return;
            foreach (var condition in _template.Default)
                BlockInternal(condition.Id, 1);
        }
        protected override void OnReset()
        {
            _blocks.Clear();
            OnBlocked = null;
            OnUnblocked = null;
            if (_template.Default == null) return;
            foreach (var condition in _template.Default)
                BlockInternal(condition.Id, 1);
            foreach (var b in _gates.Values) b.Clear();
            _gates.Clear();
        }
        protected override void OnDispose()
        {
            _blocks.Clear();
            foreach (var b in _gates.Values) b.Clear();
            _gates.Clear();
        }
        public void Block<T>(int amount = 1) where T : ICondition
            => BlockInternal(ConditionRegistry.GetId<T>(), amount);
        public void Block(ICondition condition, int amount = 1)
            => BlockInternal(condition.Id, amount);
        void BlockInternal(int id, int amount)
        {
            bool before = IsAllowedInternal(id);
            _blocks.TryGetValue(id, out int current);
            _blocks[id] = current + amount;
            NotifyIfChanged(id, before);
        }

        public bool Unblock<T>(int amount = 1) where T : ICondition
            => UnblockInternal(ConditionRegistry.GetId<T>(), amount);
        public bool Unblock(ICondition condition, int amount = 1)
            => UnblockInternal(condition.Id, amount);
        public bool UnblockAll<T>() where T : ICondition
        {
            int id = ConditionRegistry.GetId<T>();
            if (!_blocks.ContainsKey(id)) return false;
            bool before = IsAllowedInternal(id);
            _blocks.Remove(id);
            NotifyIfChanged(id, before);
            return true;
        }
        bool UnblockInternal(int id, int amount)
        {
            if (!_blocks.TryGetValue(id, out int current)) return false;
            bool before = IsAllowedInternal(id);
            int next = Math.Max(0, current - amount);
            if (next == 0) _blocks.Remove(id);
            else _blocks[id] = next;
            NotifyIfChanged(id, before);
            return true;
        }
        
        public bool IsAllowed<T>() where T : ICondition => IsAllowedInternal(ConditionRegistry.GetId<T>());
        public bool IsAllowed(ICondition condition) => IsAllowedInternal(condition.Id);
        bool IsAllowedInternal(int id)
        {
            if (_blocks.TryGetValue(id, out int count) && count > 0) return false;
            if (_gates.TryGetValue(id, out var b) && b.Count > 0 && !b.GetValue()) return false;
            return true;
        }
        public bool IsBlocked<T>() where T : ICondition
            => !IsAllowed<T>();
        public int GetBlockCount<T>() where T : ICondition
        {
            _blocks.TryGetValue(ConditionRegistry.GetId<T>(), out int count);
            return count;
        }
        
        public ExpandedBool GetGate<T>() where T : ICondition
        {
            int id = ConditionRegistry.GetId<T>();
            if (!_gates.TryGetValue(id, out var b))
            {
                b = new ExpandedBool(true);
                _gates[id] = b;
            }
            return b;
        }
        public BaseExpandedVariable<bool>.expandedDelegate AddGate<T>(Func<bool> predicate) where T : ICondition
        {
            int id = ConditionRegistry.GetId<T>();
            if (!_gates.TryGetValue(id, out var b))
            {
                b = new ExpandedBool(true);
                _gates[id] = b;
            }
            bool before = IsAllowedInternal(id);
            var handle = b.Add_Multiply(predicate);
            NotifyIfChanged(id, before);
            return handle;
        }
        public void RemoveGate<T>(BaseExpandedVariable<bool>.expandedDelegate handle) where T : ICondition
        {
            int id = ConditionRegistry.GetId<T>();
            if (!_gates.TryGetValue(id, out var b)) return;
            bool before = IsAllowedInternal(id);
            b.Remove_Multiply(handle);
            NotifyIfChanged(id, before);
        }
        void NotifyIfChanged(int id, bool before)
        {
            bool after = IsAllowedInternal(id);
            if (before == after) return;
            var condition = ConditionRegistry.GetInstance(ConditionRegistry.GetTypeById(id));
            if (!after) OnBlocked?.Invoke(condition);
            else OnUnblocked?.Invoke(condition);
        }

        public event Action<ICondition> OnBlocked;
        public event Action<ICondition> OnUnblocked;
    }

    [Serializable]
    public struct ConditionHandler : ModuleTemplate<ConditionHandlerModule>
    {
        [SerializeField][SerializeReference][SubclassSelector]
        public ACondition[] Default;
    }
}