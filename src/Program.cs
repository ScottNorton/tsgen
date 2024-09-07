// ╔═══════════════════════════════════════════════════════════════════════════════╗
//    File: Program.cs - Author: Scott Norton
// ╚═══════════════════════════════════════════════════════════════════════════════╝

using TSExportGenerator;

internal class Program {
	private static void Main(string[] args) {
		switch (args.Length) {
			case 2:
				new GenerateTypeScriptDefinitions(args[0], args[1]).Execute();
				break;
			default:
				throw new System.ArgumentException("provide paths: tsgen.exe <Assembly path> <GeneratedExports.d.ts output path>");
		}
	}
}