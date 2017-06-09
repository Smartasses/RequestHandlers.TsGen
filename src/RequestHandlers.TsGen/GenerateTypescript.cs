using System.Collections.Generic;
using RequestHandlers.Http;
using RequestHandlers.TsGen.Helpers;
using RequestHandlers.TsGen.Inheritance;
using RequestHandlers.TsGen.RequestHandlers;

namespace RequestHandlers.TsGen
{
    class GenerateTypescript
    {
        private readonly JsonDiscriminatorHelper _jsonDiscriminatorTypes;

        public GenerateTypescript(JsonDiscriminatorHelper jsonDiscriminatorTypes)
        {
            _jsonDiscriminatorTypes = jsonDiscriminatorTypes;
        }

        public IEnumerable<KeyValuePair<string, string>> GenerateContractsForRequests(HttpRequestHandlerDefinition[] definitions)
        {
            var typescriptContext = new TypescriptContext(_jsonDiscriminatorTypes);
            foreach (var httpRequestHandlerDefinition in definitions)
            {
                typescriptContext.Add(new RequestResponseTypescriptContract(httpRequestHandlerDefinition, typescriptContext));
            }
            return typescriptContext.GetFiles();
        }
    }
}
