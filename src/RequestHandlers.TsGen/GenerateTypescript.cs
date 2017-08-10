using System.Collections.Generic;
using RequestHandlers.Http;
using RequestHandlers.TsGen.Helpers;
using RequestHandlers.TsGen.RequestHandlers;

namespace RequestHandlers.TsGen
{
    class GenerateTypescript
    {
        public IEnumerable<KeyValuePair<string, string>> GenerateContractsForRequests(HttpRequestHandlerDefinition[] definitions)
        {
            var typescriptContext = new TypescriptContext();
            foreach (var httpRequestHandlerDefinition in definitions)
            {
                typescriptContext.Add(new RequestResponseTypescriptContract(httpRequestHandlerDefinition, typescriptContext));
            }
            return typescriptContext.GetFiles();
        }
    }
}
