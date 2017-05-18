using System;
using System.Linq;

namespace RequestHandlers.TsGen
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
                    Enum.GetValues(_enumType).Cast<object>(), enumValue => $@"
    {Enum.GetName(_enumType, enumValue)} = {Convert.ChangeType(enumValue, Enum.GetUnderlyingType(_enumType)).ToString()},")
                .TrimEnd(',')}
}}";
        }
    }
}