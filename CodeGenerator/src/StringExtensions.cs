namespace RealGoodApps.BlazorJavascript.CodeGenerator
{
    public static class StringExtensions
    {
        public static string[] SplitIntoLines(this string? text)
        {
            if (text == null)
            {
                return Array.Empty<string>();
            }

            var lines = new List<string>();

            using var sr = new StringReader(text);
            string? line;
            while ((line = sr.ReadLine()) != null) {
                lines.Add(line);
            }

            return lines.ToArray();
        }
    }
}
