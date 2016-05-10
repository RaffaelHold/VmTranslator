namespace VMtranslator
{
	using System;
	using System.IO;
	using System.Linq;

	class Program
	{
		private static string fileName;
		private static string givenPath;

#if DEBUG
		private static string programPath = @"C:\Users\Raffael\Dropbox\nand2tetris\nand2tetris\projects\08\FunctionCalls\SimpleFunction";
#else
		private static string programPath = AppDomain.CurrentDomain.BaseDirectory;
#endif

		static void Main(string[] args)
		{
#if DEBUG
			args[0] = "SimpleFunction.vm";
#endif

			if (args.Length < 1)
			{
				Console.WriteLine("No argument!");
				Console.ReadKey();
				return;
			}

			fileName = args[0].Split('.')[0];
			givenPath = Path.Combine(programPath, args[0]);

			if (IsDirectory())
			{
				HandleDirectory();
			}
			else if(IsFile())
			{
				HandleFile();
			}
			else
			{
				Console.WriteLine("Invalid argument!");
				Console.ReadKey();
				return;
			}

		}

		/// <summary>
		/// Checks if <see cref="givenPath"/> is a directory.
		/// </summary>
		/// <returns>Value indicating if <see cref="givenPath"/> is a directory</returns>
		private static bool IsDirectory()
		{
			return Directory.Exists(givenPath);
		}

		private static bool IsFile()
		{
			// kinda hacky but works. 

			try
			{
				File.ReadAllLines(givenPath);
			}
			catch
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Parses the supplied directory
		/// </summary>
		private static void HandleDirectory()
		{
			var files = Directory.EnumerateFiles(givenPath);
			
			foreach(var file in files.Where(p => p.Contains(".vm")))
			{
				// ToDo: implement
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Parses the supplied file
		/// </summary>
		private static void HandleFile()
		{
			string[] content = File.ReadAllLines(givenPath);

			var parser = new Parser();
			parser.Content = content;
			parser.RemoveWhitespace();

			var writer = new CodeWriter(programPath, fileName);

			//writer.WriteInit();

			while(parser.HasMoreCommands)
			{
				parser.Advance();

				switch (parser.CommandType)
				{
					case CommandType.Arithmetic:
						writer.WriteArithmetic(parser.Arg1);
						break;
					case CommandType.Call:
						writer.WriteCall(parser.Arg1, parser.Arg2 ?? 0);
						break;
					case CommandType.Function:
						writer.WriteFunction(parser.Arg1, parser.Arg2 ?? 0);
						break;
					case CommandType.Goto:
						writer.WriteGoto(parser.Arg1);
						break;
					case CommandType.If:
						writer.WriteIf(parser.Arg1);
						break;
					case CommandType.Label:
						writer.WriteLabel(parser.Arg1);
						break;
					case CommandType.Pop:
						writer.WritePushPop(parser.CommandType, parser.Arg1, parser.Arg2 ?? 0);
						break;
					case CommandType.Push:
						writer.WritePushPop(parser.CommandType, parser.Arg1, parser.Arg2 ?? 0);
						break;
					case CommandType.Return:
						writer.WriteReturn();
						break;
				}
			}

			writer.Close();
		}
	}
}
