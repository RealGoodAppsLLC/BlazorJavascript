# Overview

This tool is used to generate our Github pages in our pipeline.

It will generate a couple simple pages:
- Index page
- Stats page

The stats page will show a bunch of graphs reflecting the progress that we are making in supporting the various use cases for our binding generation. For example, every commit we run the CodeGenerator and keep track of how many interfaces are being generated. Over time, we should be generating more interfaces, since we are going to be skipping less of them as we tackle stuff such as union types, etc.

## Getting Started

```bash
# Get the list of commits in main
$ git --no-pager log --reverse --oneline --pretty='%H' > commits.txt

# Generate our HTML pages!
$ dotnet run --project src/PagesGenerator.csproj /path/to/commits.txt /path/to/commit-stats /path/to/output
```
