using System;
using System.Collections.Generic;
using System.Text;

namespace RequestHandlers.TsGen
{
    public static class CodeStr
    {
        public static string Foreach<T>(IEnumerable<T> source, Func<T, string> format)
        {
            var sb = new StringBuilder();
            foreach (var item in source)
            {
                sb.Append(format(item));
            }
            return sb.ToString();
        }

        public static string If(bool value, string ifTrue, string ifFalse = "") => value ? ifTrue : ifFalse;
        public static string Wrap(Func<string> action) => action();
    }
}