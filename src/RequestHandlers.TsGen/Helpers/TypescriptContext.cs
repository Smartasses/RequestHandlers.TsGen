using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RequestHandlers.TsGen.Dtos;
using RequestHandlers.TsGen.Enum;

namespace RequestHandlers.TsGen.Helpers
{
    class TypescriptContext
    {
        public TypescriptContext()
        {
            _files = new List<TypescriptContract>();
        }

        private readonly List<TypescriptContract> _files;

        public TypescriptContract GetTypeScriptReference(Type type)
        {
            var actualType = Nullable.GetUnderlyingType(type) ?? type;
            var currentFile = _files.SingleOrDefault(z => z.Types.Contains(actualType));
            if(currentFile != null) return currentFile;
            if (Reserved.Contains(actualType))
            {
                return null;
            }
            var dictionary = TypeHelper.GetDictionaryType(actualType);
            if (dictionary != null)
            {
                return GetTypeScriptReference(dictionary.GetGenericArguments()[1]);
            }
            var enumerable = TypeHelper.GetEnumerableType(actualType);
            if (enumerable != null)
            {
                return GetTypeScriptReference(enumerable.GetGenericArguments()[0]);
            }
            if (actualType.GetTypeInfo().IsEnum)
            {
                var enumContract = new EnumTypescriptContract(actualType, this);
                Add(enumContract);
                return enumContract;
            }
            else
            {
                var classContract = new DtoTypescriptContract(actualType, this);
                Add(classContract);
                return classContract;
            }
        }

        public void Add(TypescriptContract contract)
        {
            _files.Add(contract);

            contract.GenerateCode();
        }

        public IEnumerable<TypescriptContract> GetTypeScriptReferences(IEnumerable<Type> types)
        {
            return types.Distinct().Select(GetTypeScriptReference).Distinct().Where(x => x != null);
        }

        public IEnumerable<KeyValuePair<string, string>> GetFiles()
        {
            return _files.Select(x => new KeyValuePair<string, string>(x.FileName + ".ts", x.GenerateCode()));
        }
    }
}