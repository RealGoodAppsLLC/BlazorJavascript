using System.Collections.Immutable;
using Newtonsoft.Json;
using RealGoodApps.BlazorJavascript.PagesGenerator.Models;
using RealGoodApps.BlazorJavascript.PagesGenerator.Models.Input;

namespace RealGoodApps.BlazorJavascript.PagesGenerator
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Syntax: dotnet run --project PagesGenerator.csproj /path/to/commits.txt /path/to/commit-stats /path/to/output");
                return 1;
            }

            var commitsInputPath = args[0];
            var commitStatsDirectory = args[1];
            var outputDirectory = args[2];

            if (!File.Exists(commitsInputPath))
            {
                Console.WriteLine($"Unable to find commits input text file at \"{commitsInputPath}\"");
                return 1;
            }

            if (!Directory.Exists(commitStatsDirectory))
            {
                Console.WriteLine($"Unable to find commit stats directory at \"{commitStatsDirectory}\"");
                return 1;
            }

            // TODO: Warn the user before we blow away this directory.
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, true);
            }

            Directory.CreateDirectory(outputDirectory);

            var commitHashes = File.ReadAllLines(commitsInputPath);
            var commitInfoList = new List<CommitInfo>();

            foreach (var commitHash in commitHashes)
            {
                var commitStatSinglePath = Path.Combine(
                    commitStatsDirectory,
                    commitHash,
                    "stats.json");

                if (!File.Exists(commitStatSinglePath))
                {
                    continue;
                }

                var codeGeneratorStatsString = File.ReadAllText(commitStatSinglePath);

                var codeGeneratorStats = JsonConvert.DeserializeObject<CodeGeneratorStats>(codeGeneratorStatsString);

                if (codeGeneratorStats == null)
                {
                    throw new Exception($"Unable to parse code generator stats for \"{commitStatSinglePath}\"");
                }

                commitInfoList.Add(new CommitInfo(
                    commitHash,
                    new List<StatInfo>
                    {
                        new("Interface count", codeGeneratorStats.InterfaceCount),
                        new("Class count", codeGeneratorStats.ClassCount + codeGeneratorStats.GlobalCount + codeGeneratorStats.PrototypeCount),
                        new("Constructor implementation count", codeGeneratorStats.ConstructorImplementationCount),
                        new("Method implementation count", codeGeneratorStats.MethodImplementationCount),
                        new("Property implementation count", codeGeneratorStats.PropertyImplementationCount + codeGeneratorStats.AppendedGlobalsCount),
                        new("Indexer implementation count", codeGeneratorStats.IndexerImplementationCount),
                        new("Interface constructor count", codeGeneratorStats.InterfaceConstructorCount),
                        new("Interface method count", codeGeneratorStats.InterfaceMethodCount),
                        new("Interface property count", codeGeneratorStats.InterfacePropertyCount + codeGeneratorStats.AppendedGlobalsCount),
                        new("Interface indexer count", codeGeneratorStats.InterfaceIndexerCount),
                    }.ToImmutableList()));
            }

            File.Copy(
                Path.Combine("Content", "index.html"),
                Path.Combine(outputDirectory, "index.html"));

            var statsTemplate = File.ReadAllText(Path.Combine("Content", "stats.html"));

            File.WriteAllText(
                Path.Combine(outputDirectory, "stats.html"),
                statsTemplate.Replace("{{COMMIT_STATS_JSON}}", JsonConvert.SerializeObject(commitInfoList)));

            return 0;
        }
    }
}
