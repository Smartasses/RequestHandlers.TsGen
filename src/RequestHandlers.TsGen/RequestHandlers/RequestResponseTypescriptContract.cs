using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RequestHandlers.Http;
using RequestHandlers.TsGen.Helpers;

namespace RequestHandlers.TsGen.RequestHandlers
{
    internal class RequestResponseTypescriptContract : TypescriptContract
    {
        private readonly HttpRequestHandlerDefinition _httpRequestHandlerDefinition;

        public RequestResponseTypescriptContract(HttpRequestHandlerDefinition httpRequestHandlerDefinition, TypescriptContext ctx) : base(ctx)
        {
            _httpRequestHandlerDefinition = httpRequestHandlerDefinition;
            if (_httpRequestHandlerDefinition.Definition.ResponseType.GetTypeInfo().IsGenericType &&
                _httpRequestHandlerDefinition.Definition.ResponseType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                _httpRequestHandlerDefinition.Definition.ResponseType = _httpRequestHandlerDefinition.Definition.ResponseType.GetGenericArguments()[0];
            }
        }

        protected override string GenerateCodeInternal()
        {
            var def = _httpRequestHandlerDefinition;
            var queryStringParameters = def.Parameters.Where(x => x.BindingType == BindingType.FromQuery).ToArray();
            var routeParameters = def.Parameters.Where(x => x.BindingType == BindingType.FromRoute).ToArray();
            var bodyParameters = def.Parameters.Where(x => x.BindingType == BindingType.FromBody).ToArray();

            var requestProperties = GetProperties(def.Definition.RequestType);
            var responseProperties = GetProperties(def.Definition.ResponseType);

            var allProperties = requestProperties.Concat(responseProperties);

            var typescriptProperties = ConvertProperties(def.Parameters.Select(t => t.PropertyInfo)).ToArray();
            return $@"import {{ HttpRequest, IRequestDispatcher }} from '../common';
{Imports(allProperties.Select(x => x.PropertyType).ToArray())}
export class {def.Definition.RequestType.Name} {{{CodeStr.Foreach(typescriptProperties, prop => $@"
    public {prop.Name}: {prop.TypescriptType};")}
    constructor(init?: {{{CodeStr.Foreach(typescriptProperties, prop => 
                    $@"{prop.Name}?: {prop.TypescriptType}, ").TrimEnd(' ', ',')}}}) {{
        if(init) {{
{CodeStr.Foreach(typescriptProperties, prop => $@"
            this.{prop.Name} = init.{prop.Name};")}
        }}
    }}
    private __request = () => <HttpRequest<{def.Definition.ResponseType.Name}>>{{
        method: '{def.HttpMethod.ToString().ToLower()}',
        route: '{def.Route}'{CodeStr.Foreach(routeParameters, prop =>
                $".replace('{{{prop.PropertyName}}}', this.{CamelCase(prop.PropertyInfo.Name)}{CodeStr.If(prop.PropertyInfo.PropertyType != typeof(string), ".toString()")})")}{
        CodeStr.If((def.HttpMethod == HttpMethod.Patch || def.HttpMethod == HttpMethod.Post || def.HttpMethod == HttpMethod.Put) && bodyParameters.Any(), $@",
        body: {{{CodeStr.Foreach(bodyParameters, prop => $@"
            {CamelCase(prop.PropertyInfo.Name)}: this.{CamelCase(prop.PropertyInfo.Name)},").TrimEnd(',')}
        }}")},
        queryString: {{{CodeStr.Foreach(queryStringParameters, prop => $@"
            {CamelCase(prop.PropertyInfo.Name)}: this.{CamelCase(prop.PropertyInfo.Name)}{CodeStr.If(prop.PropertyInfo.PropertyType != typeof(string), ".toString()")},").TrimEnd(',')}
        }}
    }};
    public execute = (dispatcher: IRequestDispatcher) => dispatcher.execute(this.__request());
}}

export interface {def.Definition.ResponseType.Name} {{{CodeStr.Foreach(ConvertProperties(responseProperties), prop => $@"
    {prop.Name}: {prop.TypescriptType};")}
}}
";
        }

        public override string FileName => $"requests/{_httpRequestHandlerDefinition.Definition.RequestType.Name}";

        public override Type[] Types =>
            new[]
            {
                _httpRequestHandlerDefinition.Definition.RequestType,
                _httpRequestHandlerDefinition.Definition.ResponseType
            };
    }
}
