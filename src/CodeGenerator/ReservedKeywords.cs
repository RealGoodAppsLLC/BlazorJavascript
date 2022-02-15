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

            if (name == "string")
            {
                return "@string";
            }

            if (name == "object")
            {
                return "@object";
            }

            if (name == "this")
            {
                return "@this";
            }

            if (name == "event")
            {
                return "@event";
            }

            if (name == "continue")
            {
                return "@continue";
            }

            if (name == "lock")
            {
                return "@lock";
            }

            if (name == "ref")
            {
                return "@ref";
            }

            return name;
        }
    }
}
