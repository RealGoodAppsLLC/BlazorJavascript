namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record ParameterInfo(
        string Name,
        bool IsOptional,
        bool IsDotDotDot,
        TypeInfo Type)
    {
        public string GetNameForCSharp()
        {
            return ReservedKeywords.SanitizeName(this.Name);
        }
    }
}
