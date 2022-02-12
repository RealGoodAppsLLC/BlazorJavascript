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
            }

            return 0;
        }
    }
}
