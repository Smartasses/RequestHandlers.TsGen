using System;
using System.Linq;
using RequestHandlers.TsGen.Helpers;

namespace RequestHandlers.TsGen.Enum
{
    internal class EnumTypescriptContract : TypescriptContract
    {
        private readonly Type _enumType;

        public EnumTypescriptContract(Type enumType, TypescriptContext typescriptContext) : base(typescriptContext)
        {
            _enumType = enumType;
        }

        public override string FileName => $"enums/{_enumType.Name}";
        public override Type[] Types => new[] {_enumType};
        protected override string GenerateCodeInternal()
        {
            return $@"export enum {_enumType.Name} {{{CodeStr.Foreach(
                    System.Enum.GetValues(_enumType).Cast<object>(), enumValue => $@"
    {System.Enum.GetName(_enumType, enumValue)} = {Convert.ChangeType(enumValue, System.Enum.GetUnderlyingType(_enumType)).ToString()},")
                .TrimEnd(',')}
}}
";
        }
    }
}