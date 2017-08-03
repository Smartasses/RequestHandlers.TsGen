#if NET45
using System;
using Fclp;
using Fclp.Internals.Extensions;
using System.Collections.Generic;

namespace RequestHandlers.TsGen
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = new FluentCommandLineParser();
            var path = string.Empty;
            var inputPaths = new List<string>();
            p.Setup<string>('o')
                .Callback(record => path = record)
                .Required();
            p.Setup<List<string>>('a', "assemblies")
                .Callback(paths => inputPaths = paths)
                .Required();

            var commandLineParserResult = p.Parse(args);
            if (commandLineParserResult.HasErrors)
            {
                Console.WriteLine(commandLineParserResult.ErrorText);
                return;
            }
            new GenerateTypescriptCommand().Execute(inputPaths, path);
        }
    }
}
#endif