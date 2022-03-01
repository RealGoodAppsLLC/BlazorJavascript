# Overview

This is a script to compare the version of Typescript currently being used by TSDumper with the latest Typescript release.
If there is a later Typescript version available, this script will provide the bumped version to the pipeline.

## Getting Started

```bash
# Install dependencies
$ npm install

# Run the version checker
$ project_ts_version=$(cat ../TSDumper/package.json | jq -r '.dependencies.typescript')
$ latest_ts_version=$(npm view typescript version)
$ interop_version=$(xml sel -t -m /Project/PropertyGroup/BlazorJavascriptInteropVersion -v . ../Interop/Directory.Build.props)
$ npm run check -- $project_ts_version $latest_ts_version $interop_version
```
