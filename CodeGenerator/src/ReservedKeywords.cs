namespace RealGoodApps.BlazorJavascript.CodeGenerator
{
    public static class ReservedKeywords
    {
        public static string SanitizeName(string name)
        {
            // FIXME: This is hacky, but it works.
            name = name.Replace("$", "moneySign");

            return name switch
            {
                "string" => "@string",
                "object" => "@object",
                "float" => "@float",
                "double" => "@double",
                "int" => "@int",
                "is" => "@is",
                "public" => "@public",
                "default" => "@default",
                "checked" => "@checked",
                "as" => "@as",
                "operator" => "@operator",
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