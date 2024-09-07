# tsgen

A utility to generate TypeScript type declaration files (`*.d.ts`) for C# members decorated with `[JSExport]` in a browser-wasm project. This tool scans the C# source code and produces a `.d.ts` file, allowing strong typing of members across codebases.

---

### Why wasn't reflection used?

Due to **browser limitations** with WebAssembly, reflection—typically available in desktop .NET—is not fully supported in browser-based environments, and vice versa. If you attempt to load browser-specific assemblies or libraries in a traditional desktop .NET application, you will encounter **platform exceptions**. Also, using reflection wouldn't help in extracting code comments!

---

## Features

- **Automatic TypeScript definitions** for `[JSExport]` decorated C# methods.
- Translates C# types to their **TypeScript equivalents**, including async methods.
- Includes C# method comments as **JSDoc annotations** in the TypeScript definitions.
- Handles **namespaces**, classes, and method signatures with parameters.
- Supports **unreleased .NET features** not yet available in standard tooling.

---

## Usage

Run the `tsgen` tool from the command line to generate TypeScript definitions.

In this example, the tool is located in the target project folder. The first parameter is the path to the C# project, and the second parameter specifies the destination for the generated TypeScript declaration file.

Be sure to **change the value of the `Namespace`** in the tool to match your desired namespace (e.g., `VoxelML`) prior to building and running.

```powershell
PS C:\Project\tsgen> ./tsgen ../ ../tstype
