# tsgen

A utility to generate TypeScript type declaration files (`*.d.ts`) for C# members decorated with `[JSExport]` in a browser-wasm project. This tool scans the C# source code and produces a `.d.ts` file, allowing strong typing of members across code bases.

### Why wasn't reflection used?
Well, due to **browser limitations** with WebAssembly that are typically available in desktop .NET are not fully supported in browser-based environments and vice-versa. If you attempt to load browser-specific assemblies or libraries in a traditional desktop .NET application, you will encounter **platform exceptions**. I didn't have the time to work around this. Plus, how else would you get the code comments?

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
PS C:\Project\tsgen> ./tsgen ../ ../tstype
```

### Explanation in "Why not reflection?":
This section explains that due to **browser limitations** with WebAssembly, features like **reflection** that are typically available in desktop .NET are not fully supported in browser-based environments. It highlights the performance, security, and static nature of WebAssembly and why **static analysis** is used to generate TypeScript definitions instead.

