using System;
using System.Linq;
using System.Reflection;
using RequestHandlers.TsGen.Helpers;

namespace RequestHandlers.TsGen.Inheritance
{
    internal class DtoInheritanceTypescriptContract : TypescriptContract
    {
        private readonly Type _dtoType;
        private readonly JsonDiscriminatorHelper.Result _jsonDiscriminatorType;

        public DtoInheritanceTypescriptContract(Type dtoType, TypescriptContext typescriptContext, JsonDiscriminatorHelper.Result jsonDiscriminatorType) : base(typescriptContext)
        {
            _dtoType = dtoType;
            _jsonDiscriminatorType = jsonDiscriminatorType;
        }

        public override string FileName => $"dto/{_dtoType.Name}";
        public override Type[] Types => new[] { _dtoType };
        protected override string GenerateCodeInternal()
        {
            var properties = GetProperties(_dtoType);
            var enumName = System.Enum.GetName(_jsonDiscriminatorType.DiscriminatorType,
                _jsonDiscriminatorType.Mapping.Single(x => x.Value == _dtoType).Key);

            var interfaceBaseTypes = _jsonDiscriminatorType.BaseTypes.Where(x => x.GetTypeInfo().IsAbstract || x.GetTypeInfo().IsInterface).ToArray();
            return $@"{Imports(properties.Select(x => x.PropertyType).Concat(interfaceBaseTypes).ToArray())}
export class {_dtoType.Name}{CodeStr.If(interfaceBaseTypes.Any(), $" implements {string.Join(", ", interfaceBaseTypes.Select(x => x.Name))}")} {{{CodeStr.Foreach(ConvertProperties(properties), prop => $@"{CodeStr.If(CamelCase(_jsonDiscriminatorType.DiscriminatorPropertyName) == prop.Name, $@"
    static {prop.Name} = {prop.TypescriptType}.{enumName};
    {prop.Name} = {_dtoType.Name}.{prop.Name};", $@"
    {prop.Name}: {prop.TypescriptType};")}")}
}}";
        }
    }
}