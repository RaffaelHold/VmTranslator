using System;
using System.Collections.Generic;
using System.Linq;

namespace VMtranslator
{
	/// <summary>
	/// The commands of the VM
	/// </summary>
	public enum CommandType{
		None,
		Arithmetic,
		Push,
		Pop,
		Label,
		Goto,
		If,
		Function,
		Return,
		Call
	}

	public class Parser
	{
		private int index = -1;
		private string currentCommand;
		private string[] content;

		public CommandType CommandType;
		public string Arg1;
		public int? Arg2;
		public bool HasMoreCommands = true;

		public string[] Content
		{
			get
			{
				return content;
			}
			set
			{
				if(value == null)
				{
					content = new string[0];
				}
				else
				{
					content = value;
				}
			}
		}

		/// <summary>
		/// Removes all whitespace from <see cref="Content"/>.
		/// </summary>
		public void RemoveWhitespace()
		{
			var indizesToRemove = new List<int>();
			var contentList = content.ToList();

			for (int i = 0; i < contentList.Count; i++)
			{
				if (String.IsNullOrWhiteSpace(contentList[i]))
				{
					indizesToRemove.Add(i);
				}
				else if (contentList[i].IndexOf("/") == 0)
				{
					indizesToRemove.Add(i);
				}
				else if (content[i].IndexOf("/") != -1 && contentList[i].IndexOf("/") != 0)
				{
					int index = contentList[i].IndexOf("/");
					contentList[i] = contentList[i].Substring(0, index);
				}

				contentList[i] = contentList[i]?.Trim();
			}

			// Reverse the list so the indizes stay correct after removing elements.
			foreach (var index in indizesToRemove.Reverse<int>())
			{
				contentList.RemoveAt(index);
			}

			Content = contentList.ToArray();
		}

		/// <summary>
		/// Advances the parser to the next command.
		/// </summary>
		public void Advance()
		{
			if(content.Length == 0)
			{
				HasMoreCommands = false;
				return;
			}

			index++;
			currentCommand = content[index];

			// If the command arrays length is < 3 insert null values to avoid IndexOutOfRangeExceptions
			var command = currentCommand.Split(' ');
			if(command.Length == 1)
			{
				command = new string[] { command[0], null, null };
			}
			else if (command.Length == 2)
			{
				command = new string[] { command[0], command[1], null };
			}

			Arg1 = command[1];
			Arg2 = ParseArg2(command[2]);
			CommandType = ParseCommandType(command[0]);

			if(content.Length - 1 == index)
			{
				HasMoreCommands = false;
			}
		}

		/// <summary>
		/// Parses the type of the current command.
		/// </summary>
		/// <param name="type">Command type as string</param>
		/// <returns>Parsed <see cref="CommandType"/> value</returns>
		private CommandType ParseCommandType(string type)
		{
			if (type == "add" ||
				type == "sub" ||
				type == "neg" ||
				type == "eq"  ||
				type == "gt"  ||
				type == "lt"  ||
				type == "and" ||
				type == "or"  ||
				type == "not")
			{
				Arg1 = type;
				return CommandType.Arithmetic;
			}
			else if(type == "push")
			{
				return CommandType.Push;
			}
			else if(type == "pop")
			{
				return CommandType.Pop;
			}
			else if(type == "call")
			{
				return CommandType.Call;
			}
			else if (type == "function")
			{
				return CommandType.Function;
			}
			else if (type == "goto")
			{
				return CommandType.Goto;
			}
			else if (type == "if-goto")
			{
				return CommandType.If;
			}
			else if (type == "label")
			{
				return CommandType.Label;
			}
			else if (type == "return")
			{
				return CommandType.Return;
			}

			return CommandType.None;
		}

		/// <summary>
		/// Parse the second argument of a command.
		/// </summary>
		/// <param name="arg2">Second command argument</param>
		/// <returns>Null or parsed value</returns>
		private int? ParseArg2(string arg2)
		{
			int result;

			if (arg2 == null)
			{
				return null;
			}

			if(Int32.TryParse(arg2, out result))
			{
				return result;
			}
			else
			{
				return null;
			}
		}
	}
}
