using Newtonsoft.Json;
using RealGoodApps.BlazorJavascript.CodeGenerator.Models;
using RealGoodApps.ValueImmutableCollections;

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
            var outputStatsPath = args.Length > 2 ? args[2] : string.Empty;

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
            Directory.CreateDirectory(Path.Combine(outputDirectory, "Prototypes"));
            Directory.CreateDirectory(Path.Combine(outputDirectory, "Globals"));
            Directory.CreateDirectory(Path.Combine(outputDirectory, "Interfaces"));
            Directory.CreateDirectory(Path.Combine(outputDirectory, "Javascript"));
            Directory.CreateDirectory(Path.Combine(outputDirectory, "Factories"));
            Directory.CreateDirectory(Path.Combine(outputDirectory, "BuiltIns"));
            Directory.CreateDirectory(Path.Combine(outputDirectory, "Extensions"));

            var typeDefinitionFiles = new[]
            {
                "lib.dom.d",
                "lib.es5.d",
            };

            var parsedInfoList = new List<ParsedInfo>();

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

                parsedInfoList.Add(parsedInfo);
            }

            var merger = new ParsedInfoMerger(parsedInfoList.ToValueImmutableList());
            var mergedParseInfo = merger.Merge();

            var generator = new CodeGenerator(
                mergedParseInfo,
                outputDirectory);

            generator.Generate();

            Console.WriteLine("Generated interop code!");
            Console.WriteLine("---");
            Console.WriteLine($"Interface count: {generator.InterfaceCount}");
            Console.WriteLine($"Global count: {generator.GlobalCount}");
            Console.WriteLine($"Prototype count: {generator.PrototypeCount}");
            Console.WriteLine($"Constructor implementation count: {generator.ConstructorImplementationCount}");
            Console.WriteLine($"Method implementation count: {generator.MethodImplementationCount}");
            Console.WriteLine($"Property implementation count: {generator.PropertyImplementationCount}");
            Console.WriteLine($"Interface constructor count: {generator.InterfaceConstructorCount}");
            Console.WriteLine($"Interface method count: {generator.InterfaceMethodCount}");
            Console.WriteLine($"Interface property count: {generator.InterfacePropertyCount}");
            Console.WriteLine($"Appended globals count: {generator.AppendedGlobalsCount}");

            if (!string.IsNullOrWhiteSpace(outputStatsPath))
            {
                var outputStatsDirectory = Path.GetDirectoryName(outputStatsPath);

                if (!string.IsNullOrWhiteSpace(outputStatsDirectory))
                {
                    Directory.CreateDirectory(outputStatsDirectory);
                }

                var stats = new GeneratedStats(
                    generator.InterfaceCount,
                    generator.GlobalCount,
                    generator.PrototypeCount,
                    generator.ConstructorImplementationCount,
                    generator.MethodImplementationCount,
                    generator.PropertyImplementationCount,
                    generator.InterfaceConstructorCount,
                    generator.InterfaceMethodCount,
                    generator.InterfacePropertyCount,
                    generator.AppendedGlobalsCount);

                File.WriteAllText(outputStatsPath, JsonConvert.SerializeObject(stats, Formatting.Indented));
            }

            return 0;
        }
    }
}
