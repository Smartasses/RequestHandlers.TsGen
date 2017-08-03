using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace RequestHandlers.TsGen.Helpers
{
    abstract class TypescriptContract
    {
        private readonly Lazy<string> _lazyCode;
        protected TypescriptContext Ctx { get; }
        public abstract string FileName { get; }
        public abstract Type[] Types { get; }
        protected TypescriptContract(TypescriptContext ctx)
        {
            Ctx = ctx;
            _lazyCode = new Lazy<string>(GenerateCodeInternal);
        }

        public string GenerateCode() => _lazyCode.Value;
        protected abstract string GenerateCodeInternal();
        protected static string CamelCase(string input) => input[0].ToString().ToLower() + input.Substring(1);

        protected IEnumerable<TypescriptProperty> ConvertProperties(IEnumerable<PropertyInfo> properties)
        {
            return properties.Select(x => new TypescriptProperty
            {
                Name = CamelCase( x.Name),
                TypescriptType = GetTypescriptType(x.PropertyType)
            });
        }

        private string GetTypescriptType(Type argPropertyType)
        {
            var actualType = Nullable.GetUnderlyingType(argPropertyType) ?? argPropertyType;
            if (Reserved.Contains(actualType)) return Reserved.GetTypescriptType(actualType);
            var dictionary = TypeHelper.GetDictionaryType(actualType);
            if (dictionary != null)
            {
                var args = dictionary.GetGenericArguments();
                if (args[0] == typeof(string)) return $"{{ [key: string] : {GetTypescriptType(args[1])}}}";
                if (args[0] == typeof(int)) return $"{{ [key: number] : {GetTypescriptType(args[1])}}}";
                return "any";
            }
            var enumerable = TypeHelper.GetEnumerableType(actualType);
            if (enumerable != null)
            {
                return $"{GetTypescriptType(enumerable.GetGenericArguments()[0])}[]";
            }
            return argPropertyType.Name;
        }

        protected string Imports(params Type[] types)
        {
            var sb = new StringBuilder();
            var references = Ctx.GetTypeScriptReferences(types);
            foreach (var propertyInfo in references)
            {
                sb.AppendLine($"import {{ {string.Join(", ", propertyInfo.Types.Select(x => x.Name)) } }} from '../{propertyInfo.FileName}';");
            }
            return sb.ToString();
        }

        protected static IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            var parameters = type.GetProperties(BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Public)
                .Where(x => !x.GetCustomAttributes<JsonIgnoreAttribute>(true).Any());
            return parameters;
        }
    }
}