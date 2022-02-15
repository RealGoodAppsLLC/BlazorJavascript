# Overview

The `Interop` project is the final package that is shipped and consumed by clients. Most of the files that are checked into this directory are for scaffolding, and prior to building this project you must run the code generator. For more information, see the `src/CodeGenerator/README.md`.

## Getting Started

```bash
# Run the code generator
$ dotnet run --project ../CodeGenerator/src/CodeGenerator.csproj ../TSDumper/output ../Interop/src/Generated

# Build the interop project
$ dotnet pack --configuration Release
```
