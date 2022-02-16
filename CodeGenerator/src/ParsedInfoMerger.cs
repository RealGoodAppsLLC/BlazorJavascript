using System.Collections.Immutable;
using RealGoodApps.BlazorJavascript.CodeGenerator.Models;

namespace RealGoodApps.BlazorJavascript.CodeGenerator
{
    public class ParsedInfoMerger
    {
        private readonly ImmutableList<ParsedInfo> _parsedInfoList;

        public ParsedInfoMerger(ImmutableList<ParsedInfo> parsedInfoList)
        {
            _parsedInfoList = parsedInfoList;
        }

        public ParsedInfo Merge()
        {
            var interfaces = MergeInterfaces();

            return new ParsedInfo(
                _parsedInfoList
                    .SelectMany(parsedInfo => parsedInfo.GlobalVariables)
                    .ToImmutableList(),
                interfaces,
                _parsedInfoList
                    .SelectMany(parsedInfo => parsedInfo.TypeAliases)
                    .ToImmutableList());
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
                        new InterfaceBodyInfo(
                            existingInterface
                                .Body
                                .Constructors
                                .AddRange(@interface.Body.Constructors),
                            existingInterface
                                .Body
                                .Properties
                                .AddRange(@interface.Body.Properties)
                                .DistinctBy(interfaceInfo => interfaceInfo.Name)
                                .ToImmutableList(),
                            existingInterface
                                .Body
                                .Methods
                                .AddRange(@interface.Body.Methods),
                            existingInterface
                                .Body
                                .Indexers
                                .AddRange(@interface.Body.Indexers),
                            existingInterface
                                .Body
                                .GetAccessors
                                .AddRange(@interface.Body.GetAccessors),
                            existingInterface
                                .Body
                                .SetAccessors
                                .AddRange(@interface.Body.SetAccessors)));

                    interfaces[@interface.Name] = combinedInfo;
                }
            }

            return interfaces.Values.ToImmutableList();
        }
    }
}
