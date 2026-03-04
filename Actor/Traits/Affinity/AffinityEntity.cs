using System;
using System.Collections.Generic;

using UnityEngine.Scripting;

namespace Sackrany.Actor.Traits.Affinity
{
    public interface IAffinity
    {
        int Id { get; }
        string Name { get; }
        IAffinity Instance { get; }
        IReadOnlyList<IAffinity> Counter { get; }
        IReadOnlyList<IAffinity> CounteredBy { get; }
        void Setup();
    }

    public abstract class AAffinity<TSelf> : IAffinity 
        where TSelf : AAffinity<TSelf>
    {
        public int Id => AffinityRegistry.GetId<TSelf>();
        public virtual string Name => string.Empty;
        public IAffinity Instance => AffinityRegistry.GetInstance<TSelf>();
        
        protected IReadOnlyList<IAffinity> _counter = Array.Empty<IAffinity>();
        protected IReadOnlyList<IAffinity> _counteredBy = Array.Empty<IAffinity>();

        public IReadOnlyList<IAffinity> Counter => _counter;
        public IReadOnlyList<IAffinity> CounteredBy => _counteredBy;
        
        protected AAffinity() { }
        public virtual void Setup() { }
        
        public sealed override bool Equals(object obj) => obj is IAffinity other && other.Id == this.Id;
        public sealed override int GetHashCode() => Id.GetHashCode();
    }
    
    public readonly struct Affinity<T> where T : IAffinity
    {
        public static readonly int Id = AffinityRegistry.GetId<T>();
        public static readonly IAffinity Instance = AffinityRegistry.GetInstance<T>();
        
        public static readonly IReadOnlyList<IAffinity> Counter = AffinityRegistry.GetInstance<T>().Counter;
        public static readonly IReadOnlyList<IAffinity> CounteredBy = AffinityRegistry.GetInstance<T>().CounteredBy;
    }

    [Preserve]
    public class Default : AAffinity<Default> { }
}