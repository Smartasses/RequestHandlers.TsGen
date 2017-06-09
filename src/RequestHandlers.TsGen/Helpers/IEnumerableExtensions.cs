using System;
using System.Collections.Generic;

namespace RequestHandlers.TsGen.Helpers
{

    internal static class IEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> callback)
        {
            foreach(var item in source)
            {
                callback(item);
            }
        }
    }
}
