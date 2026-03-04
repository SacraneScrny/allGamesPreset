namespace Sackrany.Utils.Tracer
{
    public interface ITraceable
    {
        public bool IsTracing();
        public string name {get;set;}
        public int GetInstanceID();
    }
}