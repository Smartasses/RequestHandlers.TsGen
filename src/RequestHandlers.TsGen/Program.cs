using System;
using Microsoft.Extensions.CommandLineUtils;

namespace RequestHandlers.TsGen
{
    class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "dotnet tsg"
            };
            Console.WriteLine("Request handlers Typescript Generation, type dotnet tsg --help or dotnet tsg -? for more information.");
            app.Command("generate", target =>
            {
                var assemblies = target.Option("--assembly | -a",
                    "One or more assemblies to look for request handlers",
                    CommandOptionType.MultipleValue);
                var outputFolder = target.Option("--output-folder | -o",
                    "Output folder to put the generated typescript files.",
                    CommandOptionType.SingleValue);

                target.HelpOption("-? | -help");

                target.OnExecute(() =>
                {
                    new GenerateTypescriptCommand().Execute(assemblies.Values, outputFolder.Value());
                    return 0;
                });
            });
            
            app.HelpOption("-? | -help");

            return app.Execute(args);
        }
    }
}
