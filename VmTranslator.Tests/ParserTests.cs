namespace VmTranslator.Tests
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using System.Collections.Generic;
	using VMtranslator;

	[TestClass]
	public class ParserTests
	{
		/// <summary>
		/// Test if <see cref="Parser.Content"/> gets set to an empty array if its set to null.
		/// </summary>
		[TestMethod]
		public void ContentIsSetToEmptyArrayIfSetNull()
		{
			var parser = new Parser();
			parser.Content = null;

			Assert.IsNotNull(parser.Content);
			Assert.IsTrue(parser.Content.Length == 0);
		}

		/// <summary>
		/// Test if <see cref="Parser.RemoveWhitespace"/> removes whitespace.
		/// </summary>
		[TestMethod]
		public void RemoveWhiteSpaceRemovesWhiteSpace()
		{
			var commands = new List<string>() { "", null, "/Comment"};

			var parser = new Parser();
			parser.Content = commands.ToArray();

			parser.RemoveWhitespace();

			Assert.IsTrue(parser.Content.Length == 0);
		}

		/// <summary>
		/// Test if <see cref="Parser.RemoveWhitespace"/> removes inline comments.
		/// </summary>
		[TestMethod]
		public void RemoveWhiteSpaceRemovesInlineComments()
		{
			var commands = new List<string>() { "push Local 5 /Comment" };

			var parser = new Parser();
			parser.Content = commands.ToArray();

			parser.RemoveWhitespace();

			Assert.IsTrue(parser.Content[0] == "push Local 5");
		}

		/// <summary>
		/// Test if <see cref="Parser.RemoveWhitespace"/> trims spaces.
		/// </summary>
		[TestMethod]
		public void RemoveWhiteSpaceTrimsSpaces()
		{
			var commands = new List<string>() { "push Local 5 " };

			var parser = new Parser();
			parser.Content = commands.ToArray();

			parser.RemoveWhitespace();

			Assert.IsTrue(parser.Content[0] == "push Local 5");
		}

		/// <summary>
		/// Test if <see cref="Parser.Advance"/> works correctly if <see cref="Parser.Content"/> is set to null.
		/// </summary>
		[TestMethod]
		public void AdvanceIfContentIsNull()
		{
			var parser = new Parser();
			parser.Content = null;

			parser.Advance();

			Assert.IsFalse(parser.HasMoreCommands);
		}

		/// <summary>
		/// Test if <see cref="Parser.Advance"/> advances to the next command.
		/// </summary>
		[TestMethod]
		public void AdvanceAdvancesToNextCommand()
		{
			var commands = new List<string>() { "push Local 5 ", "push Local 3" };

			var parser = new Parser();
			parser.Content = commands.ToArray();

			parser.Advance();

			Assert.IsTrue(parser.CommandType == CommandType.Push && parser.Arg1 == "Local" && parser.Arg2 == 5);
		}

		/// <summary>
		/// Test if <see cref="Parser.HasMoreCommands" gets set correctly/>.
		/// </summary>
		[TestMethod]
		public void AdvanceSetsHasNextCommandTrue()
		{
			var commands = new List<string>() { "push Local 5 ", "push Local 3" };

			var parser = new Parser();
			parser.Content = commands.ToArray();

			parser.Advance();

			Assert.IsTrue(parser.HasMoreCommands);
		}

		/// <summary>
		/// Test if <see cref="Parser.HasMoreCommands" gets set correctly/>.
		/// </summary>
		[TestMethod]
		public void AdvanceSetsHasNextCommandFalse()
		{
			var commands = new List<string>() { "push Local 5 " };

			var parser = new Parser();
			parser.Content = commands.ToArray();

			parser.Advance();

			Assert.IsFalse(parser.HasMoreCommands);
		}
	}
}
