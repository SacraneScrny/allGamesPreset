using System.Collections.Generic;

namespace Sackrany.Extensions
{
    public static class QueueExtensions
    {
        public static Queue<T> EnqueueAll<T>(this Queue<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
                collection.Enqueue(item);
            return collection;
        }
    }
}