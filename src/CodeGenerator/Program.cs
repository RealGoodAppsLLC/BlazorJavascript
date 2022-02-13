using Newtonsoft.Json;
using RealGoodApps.BlazorJavascript.CodeGenerator.Models;

namespace RealGoodApps.BlazorJavascript.CodeGenerator
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Syntax: dotnet CodeGenerator.dll <path to output directory from TSDumper> <output directory>");
                return 1;
            }

            var tsDumperOutputDirectory = args[0];
            var outputDirectory = args[1];

            if (!Directory.Exists(tsDumperOutputDirectory))
            {
                Console.WriteLine($"The TSDumper output directory specified ({tsDumperOutputDirectory}) does not exist.");
                return 1;
            }

            // TODO: Before we nuke the directory, we should probably warn the user.
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, true);
            }

            Directory.CreateDirectory(outputDirectory);

            var typeDefinitionFiles = new[]
            {
                "lib.dom.d",
            };

            foreach (var typeDefinitionFile in typeDefinitionFiles)
            {
                var typeDefinitionFilePath = Path.Combine(
                    tsDumperOutputDirectory,
                    $"{typeDefinitionFile}.json");

                if (!File.Exists(typeDefinitionFilePath))
                {
                    Console.WriteLine($"Unable to find {typeDefinitionFilePath}, exiting!");
                    return 1;
                }

                var parsedInfo = JsonConvert.DeserializeObject<ParsedInfo>(File.ReadAllText(typeDefinitionFilePath));

                if (parsedInfo == null)
                {
                    Console.WriteLine($"The type definition file ({typeDefinitionFilePath}) is not formatted properly.");
                    return 1;
                }
                var generator = new Generators.CodeGenerator(
                    parsedInfo,
                    outputDirectory);

                generator.Generate();
            }

            return 0;
        }
    }
}
