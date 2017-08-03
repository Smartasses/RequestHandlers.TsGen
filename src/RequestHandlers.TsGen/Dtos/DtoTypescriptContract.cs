using System;
using System.Linq;
using RequestHandlers.TsGen.Helpers;

namespace RequestHandlers.TsGen.Dtos
{
    internal class DtoTypescriptContract : TypescriptContract
    {
        private readonly Type _dtoType;

        public DtoTypescriptContract(Type dtoType, TypescriptContext typescriptContext) : base(typescriptContext)
        {
            _dtoType = dtoType;
        
        }

        public override string FileName => $"dto/{_dtoType.Name}";
        public override Type[] Types => new[] { _dtoType };
        protected override string GenerateCodeInternal()
        {
            var properties =GetProperties(_dtoType).ToArray();
            return $@"{Imports(properties.Select(x => x.PropertyType).ToArray())}
export interface {_dtoType.Name} {{{CodeStr.Foreach(ConvertProperties(properties), prop => $@"
    {prop.Name}: {prop.TypescriptType};")}
}}
";
        }
    }
}