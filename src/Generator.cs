// ╔═══════════════════════════════════════════════════════════════════════════════╗
//    File: Generator.cs - Author: Scott Norton
// ╚═══════════════════════════════════════════════════════════════════════════════╝

namespace TSExportGenerator {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;

	public partial class GenerateTypeScriptDefinitions(string csharpProjectPath, string tsProjectPath) {
		string Namespace = "VoxelML";

		[GeneratedRegex(@"<param name=""(.+?)"">(.+?)</param>")]
		private static partial Regex ParameterName();

		[GeneratedRegex(@"class\s+(\w+)")]
		private static partial Regex ClassName();

		public void Execute() {
			Dictionary<string, List<string>> tsDefinitions = [];

			foreach (string file in Directory.GetFiles(csharpProjectPath, "*.cs", SearchOption.AllDirectories)) {
				Console.WriteLine($"[tsgen] Processing file: {file}");

				if (Path.GetFileName(Path.GetDirectoryName(file)).All(char.IsLower))
					continue;

				string[] fileLines = File.ReadAllLines(file);
				string currentClassName = null;

				for (int i = 0; i < fileLines.Length; i++) {
					string line = fileLines[i].Trim();

					if (line.StartsWith("namespace") && !line.Contains(Namespace))
						break;

					if (line.StartsWith("public") && line.Contains("class"))
						currentClassName = ExtractClassName(line);

					if (!line.Contains("[JSExport]") || string.IsNullOrEmpty(currentClassName))
						continue;

					string methodComment = ExtractMethodComment(fileLines, i - 1);
					string tsDefinition = ParseMethodSignature(fileLines, ref i);

					if (tsDefinition == null)
						continue;

					if (!tsDefinitions.ContainsKey(currentClassName))
						tsDefinitions[currentClassName] = [];

					if (methodComment != null)
						tsDefinitions[currentClassName].Add(methodComment);

					tsDefinitions[currentClassName].Add(tsDefinition);
				}
			}

			if (tsDefinitions.Count > 0)
				this.WriteGeneratedTypeScriptDefinition(tsDefinitions);
		}

		private static string ExtractClassName(string line) {
			Match match = ClassName().Match(line);
			return match.Success ? match.Groups[1].Value : null;
		}

		private static string ParseMethodSignature(string[] fileLines, ref int currentLineIndex) {
			string methodSignature = "";
			string currentLine = fileLines[currentLineIndex].Trim();

			while (currentLine.StartsWith('[') || string.IsNullOrWhiteSpace(currentLine)) {
				currentLineIndex++;
				if (currentLineIndex >= fileLines.Length)
					return null;

				currentLine = fileLines[currentLineIndex].Trim();
			}

			while (!currentLine.Contains('{') && !currentLine.EndsWith(';')) {
				methodSignature += " " + currentLine;
				currentLineIndex++;
				if (currentLineIndex >= fileLines.Length)
					return null;

				currentLine = fileLines[currentLineIndex].Trim();
			}

			methodSignature += " " + currentLine.Trim();
			int openParenIndex = methodSignature.IndexOf('(');
			int closeParenIndex = methodSignature.LastIndexOf(')');
			if (openParenIndex == -1 || closeParenIndex == -1)
				return null;

			string parameterString = methodSignature.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1).Trim();
			string[] methodInfo = methodSignature[..openParenIndex].Trim().Split(' ');

			if (methodInfo.Length < 2)
				return null;

			string returnType = methodInfo[^2];
			string methodName = methodInfo[^1];
			bool isAsync = methodInfo.Contains("async");

			returnType = isAsync ? (returnType.StartsWith("Task") ? TranslateAsyncType(returnType) : "Promise<void>") : TranslateType(returnType);
			string parameters = string.Join(", ", ParseParameters(parameterString));

			return $"{methodName}({parameters}): {returnType};";
		}

		private static string TranslateAsyncType(string taskType) =>
			taskType.Contains("Task<") ? $"Promise<{TranslateType(taskType[5..^1])}>" : "Promise<void>";

		private static IEnumerable<string> ParseParameters(string parameterString) =>
			parameterString.Split(',', StringSplitOptions.RemoveEmptyEntries)
						   .Select(param => ToTSType(param.Trim()));

		private static string ToTSType(string parameter) {
			int lastSpace = parameter.LastIndexOf(' ');
			if (lastSpace == -1) return "";
			var (csharpType, parameterName) = (parameter[..lastSpace].Trim(), parameter[(lastSpace + 1)..].Trim());

			if (csharpType.StartsWith("params"))
				return $"{parameterName}: {TranslateType(csharpType[6..].Trim())}";

			return $"{parameterName}: {TranslateType(csharpType)}";
		}

		private static string TranslateType(string csharpType) =>
			(csharpType, csharpType.EndsWith("[]")) switch {
				("int", false) => "number",
				("int", true) => "Int32Array",
				("float", false) => "number",
				("float", true) => "Float32Array",
				("double", false) => "number",
				("double", true) => "Float64Array",
				("byte", false) => "number",
				("byte", true) => "Uint8Array",
				("short", false) => "number",
				("short", true) => "Int16Array",
				("ushort", false) => "number",
				("ushort", true) => "Uint16Array",
				("uint", false) => "number",
				("uint", true) => "Uint32Array",
				("long", false) => "bigint",
				("long", true) => "BigInt64Array",
				("ulong", false) => "bigint",
				("ulong", true) => "BigUint64Array",
				("string", false) => "string",
				("string", true) => "string[]",
				("bool", false) => "boolean",
				("bool", true) => "boolean[]",
				("void", _) => "void",
				_ => "any"
			};

		private static string ExtractMethodComment(string[] fileLines, int methodLineIndex) {
			List<string> commentLines = [];
			string methodLine = fileLines[methodLineIndex + 1];
			string indentation = methodLine[..^methodLine.TrimStart().Length];

			for (int i = methodLineIndex; i >= 0; i--) {
				string line = fileLines[i].Trim();
				if (!line.StartsWith("///")) break;
				commentLines.Insert(0, line[3..].Trim());
			}

			if (commentLines.Count > 0)
				return CommentToJSDoc(commentLines, indentation);

			return null;
		}

		private static string CommentToJSDoc(List<string> commentLines, string indentation) {
			List<string> jsDoc = ["", $"{indentation}/**"];
			foreach (string line in commentLines) {
				if (string.IsNullOrWhiteSpace(line)) continue;
				if (line.StartsWith("<summary>"))
					jsDoc.Add($"{indentation} * {line.Replace("<summary>", "").Replace("</summary>", "").Trim()}");

				else if (line.StartsWith("<param")) {
					Match match = ParameterName().Match(line);
					if (match.Success)
						jsDoc.Add($"{indentation} * @param {match.Groups[1].Value} {match.Groups[2].Value}");
				}
				else if (line.StartsWith("<returns>"))
					jsDoc.Add($"{indentation} * @returns {line.Replace("<returns>", "").Replace("</returns>", "").Trim()}");
				else
					jsDoc.Add($"{indentation} * {line.Replace("</summary>", "")}");
			}
			jsDoc.Add($"{indentation} */");

			return string.Join(Environment.NewLine, jsDoc);
		}

		private void WriteGeneratedTypeScriptDefinition(Dictionary<string, List<string>> tsDefinitions) {
			string tsFilePath = Path.Combine(tsProjectPath, "dotnetEx.d.ts");

			try {
				Directory.CreateDirectory(Path.GetDirectoryName(tsFilePath) ?? throw new InvalidOperationException("Invalid directory path."));
				using StreamWriter writer = new(tsFilePath);
				writer.WriteLine("/** Automatically generated for C# JSExport decorated members. */");

				writer.WriteLine("export module dotnetEx {");
				writer.WriteLine($"\texport interface {Namespace} {{");
				foreach ((string className, List<string> methods) in tsDefinitions) {
					writer.WriteLine($"\t\t{className}: {{");
					foreach (string method in methods) {
						writer.WriteLine($"\t\t\t{method}");
					}
					writer.WriteLine("\t\t};");
				}
				writer.WriteLine("\t}");
				writer.WriteLine();
				writer.WriteLine($"\tconst {Namespace}: {Namespace};");
				writer.WriteLine("}");
				writer.WriteLine();
				writer.WriteLine("export type dotnetExports = typeof dotnetEx;");
			}
			catch (Exception ex) {
				Console.WriteLine($"[tsgen] Error: Failed to write TypeScript definitions. {ex.Message}");
			}
		}
	}
}
