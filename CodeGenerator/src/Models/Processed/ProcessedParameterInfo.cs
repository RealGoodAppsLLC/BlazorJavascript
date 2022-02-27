namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models.Processed
{
    // TODO: Annotate this with additional information so we are able to decorate it with information about unions, intersections, etc.
    public sealed record ProcessedParameterInfo(
        ProcessedTypeReferenceInfo TypeReference,
        string Name,
        bool IsDotDotDot);
}
