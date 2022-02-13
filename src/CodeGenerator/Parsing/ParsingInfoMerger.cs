using System.Collections.Immutable;
using RealGoodApps.BlazorJavascript.CodeGenerator.Models;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Parsing;

public class ParsingInfoMerger
{
    private readonly ImmutableList<ParsedInfo> _parsedInfoList;

    public ParsingInfoMerger(ImmutableList<ParsedInfo> parsedInfoList)
    {
        _parsedInfoList = parsedInfoList;
    }

    public ParsedInfo Merge()
    {
        var globalVariables = _parsedInfoList
            .SelectMany(parsedInfo => parsedInfo.GlobalVariables)
            .ToImmutableList();

        var interfaces = MergeInterfaces();

        return new ParsedInfo(
            _parsedInfoList
                .SelectMany(parsedInfo => parsedInfo.GlobalVariables)
                .ToImmutableList(),
            interfaces);
    }

    private ImmutableList<InterfaceInfo> MergeInterfaces()
    {
        // We are (roughly) following this: https://www.typescriptlang.org/docs/handbook/declaration-merging.html#merging-interfaces
        var interfaces = new Dictionary<string, InterfaceInfo>();

        foreach (var parsedInfo in _parsedInfoList)
        {
            foreach (var @interface in parsedInfo.Interfaces)
            {
                if (!interfaces.ContainsKey(@interface.Name))
                {
                    interfaces[@interface.Name] = @interface;
                    continue;
                }

                var existingInterface = interfaces[@interface.Name];

                // FIXME: This assumes that both interfaces have the same type parameters.
                var combinedInfo = new InterfaceInfo(
                    existingInterface.Name,
                    existingInterface.ExtractTypeParametersResult,
                    existingInterface
                        .ExtendsList
                        .AddRange(@interface.ExtendsList)
                        .Distinct()
                        .ToImmutableList(),
                    existingInterface
                        .Properties
                        .AddRange(@interface.Properties)
                        .DistinctBy(interfaceInfo => interfaceInfo.Name)
                        .ToImmutableList(),
                    existingInterface
                        .Methods
                        .AddRange(@interface.Methods),
                    existingInterface
                        .Indexers
                        .AddRange(@interface.Indexers),
                    existingInterface
                        .GetAccessors
                        .AddRange(@interface.GetAccessors),
                    existingInterface
                        .SetAccessors
                        .AddRange(@interface.SetAccessors));

                interfaces[@interface.Name] = combinedInfo;
            }
        }

        return interfaces.Values.ToImmutableList();
    }
}
