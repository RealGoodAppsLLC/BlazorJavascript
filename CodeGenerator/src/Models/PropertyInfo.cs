namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record PropertyInfo(
        string Name,
        bool IsReadonly,
        TypeInfo Type)
    {
        public string GetNameForCSharp()
        {
            return ReservedKeywords.SanitizeName(this.Name);
        }
    }
}
