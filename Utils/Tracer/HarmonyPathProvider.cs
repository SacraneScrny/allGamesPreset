using System;

namespace Sackrany.Utils.Tracer
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TraceAllAttribute : Attribute { }

    public interface ITraceableProvider
    {
        ITraceable GetTraceable();
    }
}