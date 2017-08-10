using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RequestHandlers.Http;

namespace RequestHandlers.TsGen
{
    class GenerateTypescriptCommand
    {
        public void Execute(List<string> inputPaths, string outputPath)
        {
            var assemblies = LoadAssembliesHelper.Load(inputPaths);
            var requestHandlerDefinitions = RequestHandlerFinder.InAssembly(assemblies);

            var definitions = requestHandlerDefinitions.SelectMany(x => x.RequestType.GetTypeInfo()
                    .GetCustomAttributes<HttpRequestAttribute>(true),
                (definition, attribute) => new HttpRequestHandlerDefinition(attribute, definition)).ToArray();

            var files = new GenerateTypescript().GenerateContractsForRequests(definitions).Concat(new[]
            {
                new KeyValuePair<string, string>("common.ts", @"import {Observable} from 'rxjs/Observable';
export interface IRequestDispatcher {
    execute<TResponse>(request: HttpRequest<TResponse>): Observable<TResponse>;
}
export interface HttpRequest<TResponse> {
    route: string;
    queryString?: {[key: string]: string},
    body?: any,
    method: string;
}
")
            });
            new SyncFiles().DoSync(outputPath, files);
        }
    }
}