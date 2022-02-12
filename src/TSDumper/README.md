# Overview

This is a script written in TypeScript that parsed the `lib.dom.d.ts` type definition file that is packaged with TypeScript. Once the type definition file is parsed, the AST (or abstract syntax tree) is then filtered and re-structured in a way that is more easily ingested by the `BlazorJavascript` code generator. Primarily, this gives us the opportunity to ensure that the JSON that the tool outputs is serialized with reliable, strong types (since the TypeScript parser makes heavy use of dynamic nodes).

## Getting Started

```bash
# Install dependencies
$ npm install

# Dump the definition file
$ npm run dump

# Dump the definition file with pretty printing
$ npm run dump -- --pretty

# Dump the definition file with debugging
$ npm run dump -- --debug
```
