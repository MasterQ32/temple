using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using NLua;
using System.Linq;

namespace temple
{
	static class MainClass
	{
		static readonly Regex rgxVariableReplacement = new Regex(@"<\[\s*(?<var>.*?)\s*\]>", RegexOptions.Compiled | RegexOptions.Singleline);
		static readonly Regex rgxTextBlock = new Regex(@"\?>(?<text>.*?)<\?\n?", RegexOptions.Compiled | RegexOptions.Singleline);

		static readonly Dictionary<string, string> escapeCodes = new Dictionary<string, string>() {
				{ "\n", "\\n" },
				{ "\r", "\\r" },
				{ "\t", "\\t" },
				{ "\"", "\\\"" },
				{ "\'", "\\\'" },
			};

		static string LuaEscape(this string value)
		{
			value = value.Replace("\\", "\\\\");
			foreach (var rep in escapeCodes)
			{
				value = value.Replace(rep.Key, rep.Value);
			}
			return value;
		}

		static StringBuilder writer = new StringBuilder();

		static Lua currentLua;

		// temple input output
		public static int Main(string[] args)
		{
			string output = null;
			string input = null;
			if (args.Length > 1)
				output = args[1];
			if (args.Length > 0)
				input = args[0];

			var arglist = args.Skip(2).ToArray();

			string src;
			if (input != null)
			{
				using (var sr = new StreamReader(input, Encoding.UTF8))
				{
					src = sr.ReadToEnd();
				}
			}
			else {
				StringBuilder source = new StringBuilder();
				string s;
				while ((s = Console.ReadLine()) != null)
				{
					source.AppendLine(s);
				}
				src = source.ToString();
			}

			string temp = rgxVariableReplacement.Replace(src, (match) =>
			{
				return "<? print(" + match.Groups["var"].Value + "); ?>";
			});

			var dst = rgxTextBlock.Replace("?>" + temp + "<?", (match) =>
			{

				return "print(\"" + match.Groups["text"].Value.LuaEscape() + "\");\n";

			});

			using (currentLua = new Lua())
			{
				currentLua["args"] = arglist;
				currentLua.RegisterFunction("print", typeof(MainClass).GetMethod(nameof(Print)));

				currentLua.DoString(Encoding.Default.GetBytes(dst), "Template")?.FirstOrDefault()?.ToString();
			}
			currentLua = null;

			var result = writer.ToString();
			if (output != null)
			{
				using (var sw = new StreamWriter(output, false, new UTF8Encoding(false)))
				{
					sw.Write(result);
				}
			}
			else
			{
				Console.Out.Write(result);
			}

			return 0;
		}

		public static void Print(object arg)
		{
			if (arg == null)
				return;
			writer.Append(arg);
		}
	}
}
