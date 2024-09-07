# tsgen

A utility to generate TypeScript type declaration files (`*.d.ts`) for C# members decorated with `[JSExport]` in a browser-wasm project. This tool scans the C# source code and produces a `.d.ts` file, allowing strong typing of members across code bases.

## Features
- Automatically generates TypeScript definitions for `[JSExport]` decorated C# methods.
- Translates C# types to their equivalent TypeScript types, including async methods.
- Includes C# method comments as JSDoc annotations in the TypeScript definitions.
- Handles namespaces, classes, and method signatures with parameters.
- Some features supported by this tool are not yet released in .NET.

## Usage

Run the `tsgen` tool from the command line to generate TypeScript definitions.

In this example, the tool is located in the target project folder. The first parameter is the path to the C# project, and the second parameter specifies the destination for the generated TypeScript declaration file.

Be sure to **change the value of the `Namespace`** in the tool to match your desired namespace (e.g., `VoxelML`) prior to building and running.

```powershell
PS C:\VoxelML\VoxelML\VoxelML\tsgen> ./tsgen ../ ../tstype
