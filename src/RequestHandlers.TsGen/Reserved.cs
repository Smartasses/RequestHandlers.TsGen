using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace RequestHandlers.TsGen
{
    public static class Reserved
    {
        static Reserved()
        {
            Map = new Dictionary<Type, string>
            {
                {typeof(string), "string"},
                {typeof(DateTime), "Date"},
                {typeof(bool), "boolean"},
                {typeof(decimal), "number"},
                {typeof(int), "number"},
                {typeof(long), "number"},
                {typeof(short), "number"},
                {typeof(double), "number"},
                {typeof(JToken), "any"},
                {typeof(JObject), "any"},
                {typeof(object), "any"},
                {typeof(Guid), "string"},
            };
        }

        public static bool Contains(Type type) => Map.ContainsKey(type);
        public static string GetTypescriptType(Type type) => Map[type];
        private static IReadOnlyDictionary<Type, string> Map { get; }
    }
}