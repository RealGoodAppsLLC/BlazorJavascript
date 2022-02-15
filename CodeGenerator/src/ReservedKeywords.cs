namespace RealGoodApps.BlazorJavascript.CodeGenerator
{
    public static class ReservedKeywords
    {
        public static string SanitizeName(string name, bool applyCamelCase)
        {
            if (applyCamelCase)
            {
                var firstLetterUppercase = name[..1].ToUpperInvariant();
                var afterFirstLetter = name[1..];
                name = $"{firstLetterUppercase}{afterFirstLetter}";
            }

            return name switch
            {
                "string" => "@string",
                "object" => "@object",
                "this" => "@this",
                "event" => "@event",
                "continue" => "@continue",
                "lock" => "@lock",
                "ref" => "@ref",
                _ => name
            };
        }
    }
}
