using Newtonsoft.Json;
using RealGoodApps.BlazorJavascript.CodeGenerator.Models;
using RealGoodApps.BlazorJavascript.CodeGenerator.Models.Processed;
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
            Directory.CreateDirectory(Path.Combine(outputDirectory, "Classes"));
            Directory.CreateDirectory(Path.Combine(outputDirectory, "Interfaces"));

            var typeDefinitionFiles = new[]
            {
                "lib.dom.d",
                "lib.es5.d",
                "lib.es2015.promise.d",
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

            var typeSimplifier = new TypeSimplifier(mergedParseInfo);
            var simplifiedParsedInfo = typeSimplifier.Simplify();

            var processor = new ParsedInfoProcessor(simplifiedParsedInfo);
            var processedInfo = processor.Process();

            var generator = new CodeGenerator(
                processedInfo,
                outputDirectory);

            generator.Generate();

            var stats = new GeneratedStats(
                processedInfo.Interfaces.Items.Count,
                processedInfo.Classes.Items.Count,
                processedInfo.Classes.Items
                    .Sum(c => c.Implementations.Items.Sum(i => i.Constructors.Items.Count)),
                processedInfo.Classes.Items
                    .Sum(c => c.Implementations.Items.Sum(i => i.Methods.Items.Count)),
                processedInfo.Classes.Items
                    .Sum(c => c.Implementations.Items.Sum(i => i.Properties.Items.Count)),
                processedInfo.Classes.Items
                    .Sum(c => c.Implementations.Items.Sum(i => i.Indexers.Items.Count)),
                processedInfo.Interfaces.Items.Sum(i => i.Constructors.Items.Count),
                processedInfo.Interfaces.Items.Sum(i => i.Methods.Items.Count),
                processedInfo.Interfaces.Items.Sum(i => i.Properties.Items.Count),
                processedInfo.Interfaces.Items.Sum(i => i.Indexers.Items.Count));

            Console.WriteLine("Generated interop code!");
            Console.WriteLine("---");
            Console.WriteLine($"Interface count: {stats.InterfaceCount}");
            Console.WriteLine($"Class count: {stats.ClassCount}");
            Console.WriteLine($"Constructor implementation count: {stats.ConstructorImplementationCount}");
            Console.WriteLine($"Method implementation count: {stats.MethodImplementationCount}");
            Console.WriteLine($"Property implementation count: {stats.PropertyImplementationCount}");
            Console.WriteLine($"Indexer implementation count: {stats.IndexerImplementationCount}");
            Console.WriteLine($"Interface constructor count: {stats.InterfaceConstructorCount}");
            Console.WriteLine($"Interface method count: {stats.InterfaceMethodCount}");
            Console.WriteLine($"Interface property count: {stats.InterfacePropertyCount}");
            Console.WriteLine($"Interface indexer count: {stats.InterfaceIndexerCount}");

            if (!string.IsNullOrWhiteSpace(outputStatsPath))
            {
                var outputStatsDirectory = Path.GetDirectoryName(outputStatsPath);

                if (!string.IsNullOrWhiteSpace(outputStatsDirectory))
                {
                    Directory.CreateDirectory(outputStatsDirectory);
                }

                File.WriteAllText(outputStatsPath, JsonConvert.SerializeObject(stats, Formatting.Indented));
            }

            return 0;
        }
    }
}
