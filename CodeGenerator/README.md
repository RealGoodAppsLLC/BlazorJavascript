# Overview

The `CodeGenerator` project is a tool to take the output from the `TSDumper` project and generate C# classes to wrap the interop with the global variables and interfaces in Javascript.

## Getting Started

```bash
# Run the tool
$ dotnet run --project src/CodeGenerator.csproj ../TSDumper/output ../Interop/src/Generated
```
