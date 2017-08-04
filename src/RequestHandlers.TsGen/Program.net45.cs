#if NET45
using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using CommandLine.Text;

namespace RequestHandlers.TsGen
{
    class Options {
      [Option('o', "output", Required = true, HelpText = "Output directory.")]
      public string OutputDirectory { get; set; }
        
      [OptionList('a', "assemblies", Separator = ';', HelpText = "Assemblies.")]
      public IList<string> Assemblies { get; set; }
    
      [ParserState]
      public IParserState LastParserState { get; set; }
    
      [HelpOption]
      public string GetUsage() {
        return HelpText.AutoBuild(this,
          (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
      }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options)) {
                foreach(var input in options.Assemblies)
                {
                    Console.WriteLine("Input: " + input);
                }
                Console.WriteLine("Ouput: " + options.OutputDirectory);
                new GenerateTypescriptCommand().Execute(options.Assemblies.ToList(), options.OutputDirectory);
            }
        }
    }
}
#endif