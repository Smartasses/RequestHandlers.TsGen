using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RequestHandlers;
using RequestHandlers.Http;
using System.Text.RegularExpressions;
using System.Runtime.Loader;

namespace RequestHandlers.TsGen
{
    class Program
    {


        static void Main(string[] args)
        {
            string path = null;
            List<string> inputPaths = new List<string>();
            Dictionary<string, Action<string>> processors = new Dictionary<string, Action<string>>();
            processors.Add("-o", input =>  path = input);
            processors.Add("-a", input =>  inputPaths.Add(input));
            
            var enumerator = args.GetEnumerator();
            Action<string> currentProcessor = null;
            while(enumerator.MoveNext())
            {
                string currentItem = (string)enumerator.Current;
                if(processors.ContainsKey(currentItem.ToLowerInvariant()))
                {
                    currentProcessor = processors[currentItem.ToLowerInvariant()];
                }
                else if(currentProcessor != null)
                {
                    currentProcessor(currentItem);
                }
            }

            var assemblies = inputPaths.Select(x => AssemblyLoadContext.Default.LoadFromAssemblyPath(x)).ToArray();
        
            var requestHandlerDefinitions = RequestHandlerFinder.InAssembly(assemblies);
        
            var definitions = requestHandlerDefinitions.SelectMany(x => x. RequestType.GetTypeInfo()
                    .GetCustomAttributes<HttpRequestAttribute>(true),
                (definition, attribute) => new HttpRequestHandlerDefinition(attribute, definition)).ToArray();

            //var files = GeneratedFiles(definitions);
                
            var jsonDiscriminatorTypes = new JsonDiscriminatorHelper(assemblies);
            var files = new GenerateTypescript(jsonDiscriminatorTypes).GenerateContractsForRequests(definitions).Concat(new []{
            new KeyValuePair<string, string>("common.ts", @"export interface IRequestDispatcher {
    execute<TResponse>(request: HttpRequest<TResponse>):Promise<TResponse>;
}
export interface HttpRequest<TResponse> {
    route: string;
    queryString?: {[key:string]:string},
    body?: any,
    method: string;
}")
        });

            SyncFiles(path, files);

            Console.WriteLine("Generated typescript contracts");
        }
        private static void SyncFiles(string path, IEnumerable<KeyValuePair<string, string>> files)
        {
            files.ToArray();
            if(string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            var dict = files.ToDictionary(x => Path.Combine(path, x.Key), x => x.Value);
            var typescriptFiles = Directory.GetFileSystemEntries(path, "*.ts", SearchOption.AllDirectories);
            typescriptFiles.Where(x => !dict.ContainsKey(x)).ForEach(File.Delete);
            Directory.GetDirectories(path, "*", SearchOption.AllDirectories).Where(x => Directory.GetFiles(x).Length == 0).ForEach(
                x =>
                {
                    if(Directory.Exists(x))
                        Directory.Delete(x, true);
                });
            var dirHash = new HashSet<string>();
            foreach (var file in dict)
            {
                var dir = Path.GetDirectoryName(file.Key);
                if (!dirHash.Contains(dir))
                {
                    dirHash.Add(dir);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }
                File.WriteAllText(file.Key, file.Value);
            }
        }
    }
}
