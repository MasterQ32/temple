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
			string output = null;
			string input = null;
			switch (args.Length) {
			case 2:
				input = args [0];
				output = args [1];
				break;
			case 1:
				input = args [0];
				break;
			}

			string src;
			if (input != null) {
				src = File.ReadAllText (input);
			} else {
				StringBuilder source = new StringBuilder ();
				string s;
				while ((s = Console.ReadLine()) != null)
				{
					source.AppendLine (s);
				}
				src = source.ToString ();
			}

			string temp = rgxVariableReplacement.Replace (src, (match) => {
				return "<? print(" + match.Groups["var"].Value + "); ?>";
			});

			var dst = rgxTextBlock.Replace ("?>" + temp + "<?", (match) => {

				return "print(\"" + match.Groups["text"].Value.LuaEscape() + "\");\n";

			});

			writer.Append ("print = io.write;");
			writer.Append (dst);

			var proc = Process.Start (new ProcessStartInfo("lua", "-") {
				RedirectStandardOutput = true,
				RedirectStandardInput = true,
				UseShellExecute = false
			});
			proc.StandardInput.Write(writer.ToString());
			proc.StandardInput.Close ();
			proc.WaitForExit ();
			if (proc.ExitCode != 0) {
				return proc.ExitCode;
			}

			string result = proc.StandardOutput.ReadToEnd ();
			if (output != null) {
				File.WriteAllText (output, result);
			} else {
				Console.Out.Write (result);
			}

			return 0;
		}
	}
}
