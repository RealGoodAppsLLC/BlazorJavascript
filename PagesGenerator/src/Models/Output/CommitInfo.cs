using System.Collections.Immutable;

namespace RealGoodApps.BlazorJavascript.PagesGenerator.Models
{
    public sealed record CommitInfo(
        string CommitHash,
        ImmutableList<StatInfo> Stats);
}
