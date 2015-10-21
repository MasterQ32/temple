using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using NLua;

namespace temple
{
	static class MainClass
	{
		static readonly Regex rgxVariableReplacement = new Regex (@"<\[\s*(?<var>[A-Za-z_]+[A-Za-z0-9_]*)\s*\]>", RegexOptions.Compiled | RegexOptions.Singleline);
		static readonly Regex rgxTextBlock = new Regex(@"\?>(?<text>.*?)<\?\n?", RegexOptions.Compiled | RegexOptions.Singleline);

		static string LuaEscape(this string value)
		{
			Dictionary<string, string> values = new Dictionary<string, string> () {
				{ "\n", "\\n" },
				{ "\r", "\\r" },
				{ "\t", "\\t" },
				{ "\"", "\\\"" },
				{ "\'", "\\\'" },
			};
			foreach (var rep in values) {
				value = value.Replace (rep.Key, rep.Value);
			}
			return value;
		}

		static StringBuilder writer = new StringBuilder();

		// temple input output
		public static int Main (string[] args)
		{
			string output, input;
			switch (args.Length) {
			case 2:
				input = args [0];
				output = args [1];
				break;
			case 1:
				input = args [0];
				output = Path.GetDirectoryName (input);
				if (output.Length > 0) {
					output += "/";
				}
				output += Path.GetFileNameWithoutExtension (input);
				break;
			default:
				Console.Error.WriteLine ("Invalid arguments.");
				return -1;
			}

			string src = File.ReadAllText (input);

			string temp = rgxVariableReplacement.Replace (src, (match) => {
				return "<? print(" + match.Groups["var"].Value + "); ?>";
			});

			var dst = rgxTextBlock.Replace ("?>" + temp + "<?", (match) => {

				return "print(\"" + match.Groups["text"].Value.LuaEscape() + "\");\n";

			});

			writer.Append ("print = io.write;");
			writer.Append (dst);

			File.WriteAllText (output + ".lua", writer.ToString());

			Process proc = Process.Start (new ProcessStartInfo("lua", output + ".lua") {
				RedirectStandardOutput = true,
				UseShellExecute = false
			});
			proc.WaitForExit ();
			if (proc.ExitCode != 0) {
				File.Delete (output + ".lua");
				return proc.ExitCode;
			}

			File.WriteAllText(output, proc.StandardOutput.ReadToEnd ());

			File.Delete (output + ".lua");

			return 0;
		}
	}
}
