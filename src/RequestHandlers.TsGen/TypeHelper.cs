using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RequestHandlers.TsGen
{
    static class TypeHelper
    {
        public static Type GetEnumerableType(Type actualType)
        {
            return actualType.GetTypeInfo().IsGenericType && actualType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ? actualType :
                actualType.GetInterfaces().FirstOrDefault(t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }
        public static Type GetDictionaryType(Type actualType)
        {
            return actualType.GetTypeInfo().IsGenericType && actualType.GetGenericTypeDefinition() == typeof(IDictionary<,>) ? actualType :
                actualType.GetInterfaces().FirstOrDefault(t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }
    }
}