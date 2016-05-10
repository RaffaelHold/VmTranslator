namespace VMtranslator
{
	using System;
	using System.IO;

	/// <summary>
	/// Avaliable memory segments
	/// </summary>
	public enum Segment
	{
		None,
		Argument,
		Local,
		Static,
		Constant,
		This,
		That,
		Pointer,
		Temp
	}

	public class CodeWriter
	{
		private int labelCount = 0;
		private readonly StreamWriter writer;
		private readonly string name;

		// default value is Sys.Init because thats always the first function to be written
		private string functionName = "Sys.Init";

		/// <summary>
		/// Constructs a new CodeWriter.
		/// </summary>
		/// <param name="path">Output path</param>
		/// <param name="name">File name</param>
		public CodeWriter(string path, string name)
		{
			this.name = name;
			string outputFile = Path.Combine(path, name + ".asm");

			if (!File.Exists(outputFile))
			{
				var stream = File.Create(outputFile);
				writer = new StreamWriter(stream);
			}
			else
			{
				writer = new StreamWriter(outputFile, false);
			}
		}

		#region Init

		/// <summary>
		/// Writes the Sys.Init function
		/// </summary>
		public void WriteInit()
		{
			writer.WriteLine("@256");
			writer.WriteLine("D=M");
			writer.WriteLine("@SP");
			writer.WriteLine("M=D");

			writer.WriteLine("@Sys.Init");
			writer.WriteLine("0;JMP");
		}

		#endregion

		#region Label

		/// <summary>
		/// Writes a new label
		/// </summary>
		/// <param name="label">Label name</param>
		public void WriteLabel(string label)
		{
			writer.WriteLine("(" + functionName + "$" + label + ")");
		}

		#endregion

		#region Goto

		/// <summary>
		/// Writes a GoTo statement
		/// </summary>
		/// <param name="label">The label to jump to</param>
		public void WriteGoto(string label)
		{
			writer.WriteLine("@" + functionName + "$" + label);
			writer.WriteLine("0;JMP");
		}

		#endregion

		#region If

		/// <summary>
		/// Writes an if condition
		/// </summary>
		/// <param name="label">The label to jump to</param>
		public void WriteIf(string label)
		{
			PopToVar("GotoTemp");
			writer.WriteLine("@GotoTemp");
			writer.WriteLine("D=M");
			writer.WriteLine("@" + functionName + "$" + label);
			writer.WriteLine("D;JNE");
		}

		#endregion

		#region Call

		/// <summary>
		/// Writes a method call.
		/// </summary>
		/// <param name="functionName">Name of the function to call</param>
		/// <param name="numArgs">Number of passed arguments</param>
		public void WriteCall(string functionName, int numArgs)
		{
			PushSymbol(functionName + "_Return");
			PushSymbol("LCL");
			PushSymbol("ARG");
			PushSymbol("THIS");
			PushSymbol("THAT");

			// ARG = SP-n-5
			writer.WriteLine("@SP");
			writer.WriteLine("D=M");
			writer.WriteLine("@" + numArgs + 5);
			writer.WriteLine("D=D-A");
			writer.WriteLine("@ARG");
			writer.WriteLine("M=D");

			// LCL = SP
			writer.WriteLine("@SP");
			writer.WriteLine("D=M");
			writer.WriteLine("@LCL");
			writer.WriteLine("M=D");

			// jump to function
			writer.WriteLine("@" + functionName);
			writer.WriteLine("0;JMP");
			writer.WriteLine("(" + functionName + "_Return" + ")");
		}

		#endregion

		#region Return

		/// <summary>
		/// Writes a return statement.
		/// </summary>
		public void WriteReturn()
		{
			// FRAME = LCL
			writer.WriteLine("@LCL");
			writer.WriteLine("D=M");
			writer.WriteLine("@FRAME");
			writer.WriteLine("M=D");

			writer.WriteLine("@FRAME");
			writer.WriteLine("D=M");
			writer.WriteLine("@5");
			writer.WriteLine("D=D-A");
			writer.WriteLine("@RET");
			writer.WriteLine("M=D");

			// *Arg = Pop()
			writer.WriteLine("@SP");
			writer.WriteLine("A=M-1");
			writer.WriteLine("D=M");
			writer.WriteLine("@ARG");
			writer.WriteLine("A=M");
			writer.WriteLine("M=D");

			// SP = ARG + 1
			writer.WriteLine("@ARG");
			writer.WriteLine("D=M+1");
			writer.WriteLine("@SP");
			writer.WriteLine("M=D");

			// THAT = *(FRAME-1)
			writer.WriteLine("@FRAME");
			writer.WriteLine("D=M");
			writer.WriteLine("@1");
			writer.WriteLine("D=D-A");
			writer.WriteLine("@THAT");
			writer.WriteLine("M=D");

			// THIS = *(FRAME-2)
			writer.WriteLine("@FRAME");
			writer.WriteLine("D=M");
			writer.WriteLine("@2");
			writer.WriteLine("D=D-A");
			writer.WriteLine("@THIS");
			writer.WriteLine("M=D");

			// ARG = *(FRAME-3)
			writer.WriteLine("@FRAME");
			writer.WriteLine("D=M");
			writer.WriteLine("@3");
			writer.WriteLine("D=D-A");
			writer.WriteLine("@ARG");
			writer.WriteLine("M=D");

			// LCL = *(FRAME-4)
			writer.WriteLine("@FRAME");
			writer.WriteLine("D=M");
			writer.WriteLine("@4");
			writer.WriteLine("D=D-A");
			writer.WriteLine("@LCL");
			writer.WriteLine("M=D");

			writer.WriteLine("@RET");
			writer.WriteLine("0;JMP");
		}

		#endregion

		#region Function

		/// <summary>
		/// Writes a new function and initializes the stack segments.
		/// </summary>
		/// <param name="functionName">Name of the function</param>
		/// <param name="numLocals">Number of locals</param>
		public void WriteFunction(string functionName, int numLocals)
		{
			this.functionName = functionName;

			writer.WriteLine("(" + functionName + ")");

			for (int i = 0; i < numLocals; i++)
			{
				PushConstant(0);
			}
		}

		#endregion

		#region Arithmetic

		/// <summary>
		/// Writes a arithmetic command.
		/// </summary>
		/// <param name="command">The command to write</param>
		public void WriteArithmetic(string command)
		{
			switch (command)
			{
				case "add":
					WriteAddCommand();
					break;
				case "sub":
					WriteSubCommand();
					break;
				case "neg":
					WriteNegCommand();
					break;
				case "eq":
					WriteEqCommand();
					break;
				case "gt":
					WriteGtCommand();
					break;
				case "lt":
					WriteLtCommand();
					break;
				case "and":
					WriteAndCommand();
					break;
				case "or":
					WriteOrCommand();
					break;
				case "not":
					WriteNotCommand();
					break;
				default:
					Console.WriteLine("The command \"{0}\" is not recognized", command);
					break;
			}
		}
		
		/// <summary>
		/// Writes an add command.
		/// </summary>
		private void WriteAddCommand()
		{
			// pop first argument and set new SP
			writer.WriteLine("@SP");
			writer.WriteLine("A=M-1");
			writer.WriteLine("D=M");
			writer.WriteLine("@SP");
			writer.WriteLine("M=M-1");

			// pop second argument and add it
			// set new sp
			writer.WriteLine("@SP");
			writer.WriteLine("A=M-1");
			writer.WriteLine("D=D+M");
			writer.WriteLine("@SP");
			writer.WriteLine("M=M-1");

			// push value
			writer.WriteLine("@SP");
			writer.WriteLine("A=M");
			writer.WriteLine("M=D");

			// increment SP
			writer.WriteLine("@SP");
			writer.WriteLine("M=M+1");
		}

		/// <summary>
		/// Writes a subtract command.
		/// </summary>
		private void WriteSubCommand()
		{
			// pop first argument and set new SP
			writer.WriteLine("@SP");
			writer.WriteLine("A=M-1");
			writer.WriteLine("D=M");
			writer.WriteLine("@SP");
			writer.WriteLine("M=M-1");

			// pop second argument and subtract it
			// set new sp
			writer.WriteLine("@SP");
			writer.WriteLine("A=M-1");
			writer.WriteLine("D=M-D");
			writer.WriteLine("@SP");
			writer.WriteLine("M=M-1");

			// push value
			writer.WriteLine("@SP");
			writer.WriteLine("A=M");
			writer.WriteLine("M=D");

			// increment SP
			writer.WriteLine("@SP");
			writer.WriteLine("M=M+1");
		}
		
		/// <summary>
		/// Writes a negate command.
		/// </summary>
		private void WriteNegCommand()
		{
			writer.WriteLine("@SP");
			writer.WriteLine("A=M-1");
			writer.WriteLine("M=-M");
		}
		
		/// <summary>
		/// Writes an equal command.
		/// </summary>
		private void WriteEqCommand()
		{
			WriteCompareCommand("JEQ");
		}

		/// <summary>
		/// writes a greater than command.
		/// </summary>
		private void WriteGtCommand()
		{
			WriteCompareCommand("JGT");
		}

		/// <summary>
		/// Writes a lesser than command.
		/// </summary>
		private void WriteLtCommand()
		{
			WriteCompareCommand("JLT");
		}

		/// <summary>
		/// Writes a compare command.
		/// </summary>
		/// <param name="jump"></param>
		private void WriteCompareCommand(string jump)
		{
			// pop first argument and set new SP
			writer.WriteLine("@SP");
			writer.WriteLine("A=M-1");
			writer.WriteLine("D=M");
			writer.WriteLine("@SP");
			writer.WriteLine("M=M-1");

			// pop second argument and subtract it
			// set new sp
			writer.WriteLine("@SP");
			writer.WriteLine("A=M-1");
			writer.WriteLine("D=M-D");
			writer.WriteLine("@SP");
			writer.WriteLine("M=M-1");

			// Compare and jump
			writer.WriteLine("@" + name + "." + "JMP" + labelCount);
			writer.WriteLine("D;" + jump);

			// push 0 if false
			writer.WriteLine("@SP");
			writer.WriteLine("A=M");
			writer.WriteLine("M=0");
			writer.WriteLine("@" + name + "." + "EndJMP" + labelCount);
			writer.WriteLine("0;JMP");

			// push -1 if true
			writer.WriteLine("(" + name + "." + "JMP" + labelCount + ")");
			writer.WriteLine("@SP");
			writer.WriteLine("A=M");
			writer.WriteLine("M=-1");

			writer.WriteLine("(" + name + "." + "EndJMP" + labelCount + ")");

			// increment SP
			writer.WriteLine("@SP");
			writer.WriteLine("M=M+1");

			labelCount++;
		}

		/// <summary>
		/// Writes a bitwise and command
		/// </summary>
		private void WriteAndCommand()
		{
			// pop first argument and set new SP
			writer.WriteLine("@SP");
			writer.WriteLine("A=M-1");
			writer.WriteLine("D=M");
			writer.WriteLine("@SP");
			writer.WriteLine("M=M-1");

			// pop second argument and add it
			// set new sp
			writer.WriteLine("@SP");
			writer.WriteLine("A=M-1");
			writer.WriteLine("D=D&M");
			writer.WriteLine("@SP");
			writer.WriteLine("M=M-1");

			// push value
			writer.WriteLine("@SP");
			writer.WriteLine("A=M");
			writer.WriteLine("M=D");

			// increment SP
			writer.WriteLine("@SP");
			writer.WriteLine("M=M+1");
		}

		/// <summary>
		/// Writes a bitwise or command
		/// </summary>
		private void WriteOrCommand()
		{
			// pop first argument and set new SP
			writer.WriteLine("@SP");
			writer.WriteLine("A=M-1");
			writer.WriteLine("D=M");
			writer.WriteLine("@SP");
			writer.WriteLine("M=M-1");

			// pop second argument and add it
			// set new sp
			writer.WriteLine("@SP");
			writer.WriteLine("A=M-1");
			writer.WriteLine("D=D|M");
			writer.WriteLine("@SP");
			writer.WriteLine("M=M-1");

			// push value
			writer.WriteLine("@SP");
			writer.WriteLine("A=M");
			writer.WriteLine("M=D");

			// increment SP
			writer.WriteLine("@SP");
			writer.WriteLine("M=M+1");
		}

		/// <summary>
		/// Writes a bitwise not command
		/// </summary>
		private void WriteNotCommand()
		{
			writer.WriteLine("@SP");
			writer.WriteLine("A=M-1");
			writer.WriteLine("M=!M");
		}

		#endregion

		#region PushPop

		/// <summary>
		/// Writes a push or pop command.
		/// </summary>
		/// <param name="commandType">Type of the command</param>
		/// <param name="seg">Memory segment which is acted on</param>
		/// <param name="index">Index of the memory address in the segment</param>
		public void WritePushPop(CommandType commandType, string seg, int index)
		{
			var parsedSegment = ParseSegment(seg);

			switch(commandType)
			{
				case CommandType.Push:
					WritePush(parsedSegment, index);
					break;
				case CommandType.Pop:
					WritePop(parsedSegment, index);
					break;
			}

		}

		/// <summary>
		/// Parses the string representation of a segment to a <see cref="Segment"/> enum value.
		/// </summary>
		/// <param name="seg">String representation of the segment </param>
		/// <returns></returns>
		private Segment ParseSegment(string seg)
		{
			Segment segment = Segment.None;

			switch (seg)
			{
				case "argument":
					segment = Segment.Argument;
					break;
				case "constant":
					segment = Segment.Constant;
					break;
				case "local":
					segment = Segment.Local;
					break;
				case "pointer":
					segment = Segment.Pointer;
					break;
				case "static":
					segment = Segment.Static;
					break;
				case "temp":
					segment = Segment.Temp;
					break;
				case "that":
					segment = Segment.That;
					break;
				case "this":
					segment = Segment.This;
					break;
				default:
					Console.WriteLine("The segment \"{0}\" was not recognized", seg);
					break;
			}

			return segment;
		}

		/// <summary>
		/// Writes a push command.
		/// </summary>
		/// <param name="segment">Segment to push the value from</param>
		/// <param name="index">Index of the memory address in the segment</param>
		private void WritePush(Segment segment, int index)
		{
			switch (segment)
			{
				case Segment.Argument:
					PushArgument(index);
					break;
				case Segment.Constant:
					PushConstant(index);
					break;
				case Segment.Local:
					PushLocal(index);
					break;
				case Segment.Pointer:
					PushPointer(index);
					break;
				case Segment.Static:
					PushStatic(index);
					break;
				case Segment.Temp:
					PushTemp(index);
					break;
				case Segment.That:
					PushThat(index);
					break;
				case Segment.This:
					PushThis(index);
					break;
			}
		}

		/// <summary>
		/// Pushes the value at the given <paramref name="index"/> in the "Argument" segment onto the stack.
		/// </summary>
		/// <param name="index">Index of the memory address in the segment</param>
		private void PushArgument(int index)
		{
			Push("ARG", index);
		}

		/// <summary>
		/// Pushes a <paramref name="value"/> onto the stack.
		/// </summary>
		/// <param name="value"></param>
		private void PushConstant(int value)
		{
			writer.WriteLine("@" + value);
			writer.WriteLine("D=A");
			writer.WriteLine("@SP");
			writer.WriteLine("A=M");
			writer.WriteLine("M=D");
			
			// Increment SP
			writer.WriteLine("@SP");
			writer.WriteLine("M=M+1");
		}

		/// <summary>
		/// Pushes the value at the given <paramref name="index"/> in the "Local" segment onto the stack.
		/// </summary>
		/// <param name="index">Index of the memory address in the segment</param>
		private void PushLocal(int index)
		{
			Push("LCL", index);
		}

		/// <summary>
		/// Pushes the value at the given <paramref name="index"/> in the "This" segment onto the stack.
		/// </summary>
		/// <param name="index">Index of the memory address in the segment</param>
		private void PushThis(int index)
		{
			Push("THIS", index);
		}

		/// <summary>
		/// Pushes the value at the given <paramref name="index"/> in the "That" segment onto the stack.
		/// </summary>
		/// <param name="index">Index of the memory address in the segment</param>
		private void PushThat(int index)
		{
			Push("THAT", index);
		}

		/// <summary>
		/// Pushes the value at the given <paramref name="index"/> in the "Pointer" segment onto the stack.
		/// </summary>
		/// <param name="index">Index of the memory address in the segment</param>
		private void PushPointer(int index)
		{
			// Select memory
			writer.WriteLine("@R" + (3 + index));
			writer.WriteLine("D=M");

			// Push value onto stack
			writer.WriteLine("@SP");
			writer.WriteLine("A=M");
			writer.WriteLine("M=D");

			// Increment SP
			writer.WriteLine("@SP");
			writer.WriteLine("M=M+1");
		}

		/// <summary>
		/// Pushes the value at the given <paramref name="index"/> in the "Temp" segment onto the stack.
		/// </summary>
		/// <param name="index">Index of the memory address in the segment</param>
		private void PushTemp(int index)
		{
			// Select memory
			writer.WriteLine("@R" + (5 + index));
			writer.WriteLine("D=M");

			// Push value onto stack
			writer.WriteLine("@SP");
			writer.WriteLine("A=M");
			writer.WriteLine("M=D");

			// Increment SP
			writer.WriteLine("@SP");
			writer.WriteLine("M=M+1");
		}

		/// <summary>
		/// Pushes the value at the given <paramref name="index"/> in the "Static" segment onto the stack.
		/// </summary>
		/// <param name="index">Index of the memory address in the segment</param>
		private void PushStatic(int index)
		{
			// Select memory
			writer.WriteLine("@" + name + "." + index);
			writer.WriteLine("D=M");

			// Push value onto stack
			writer.WriteLine("@SP");
			writer.WriteLine("A=M");
			writer.WriteLine("M=D");

			// Increment SP
			writer.WriteLine("@SP");
			writer.WriteLine("M=M+1");
		}

		/// <summary>
		/// Pushes the value of a symbol onto the stack.
		/// </summary>
		/// <param name="index">Name of the symbol to push</param>
		private void PushSymbol(string name)
		{
			// Select memory
			writer.WriteLine("@" + name);
			writer.WriteLine("D=M");
			writer.WriteLine("@SP");
			writer.WriteLine("M=D");

			// Increment SP
			writer.WriteLine("@SP");
			writer.WriteLine("M=M+1");
		}

		/// <summary>
		/// Pushes a value from a given <paramref name="index"/> in a <paramref name="segment"/> onto the stack.
		/// </summary>
		/// <param name="index">Index of the memory address in the segment</param>
		private void Push(string segment, int index)
		{
			// Select memory
			writer.WriteLine("@" + segment);
			writer.WriteLine("D=M");
			writer.WriteLine("@" + index);
			writer.WriteLine("A=D+A");
			writer.WriteLine("D=M");

			// Push value onto stack
			writer.WriteLine("@SP");
			writer.WriteLine("A=M");
			writer.WriteLine("M=D");

			// Increment SP
			writer.WriteLine("@SP");
			writer.WriteLine("M=M+1");
		}

		/// <summary>
		/// Writes a pop command.
		/// </summary>
		/// <param name="segment">Segment to pop the value into</param>
		/// <param name="index">Index of the memory address in the segment</param>
		private void WritePop(Segment segment, int index)
		{
			switch (segment)
			{
				case Segment.Argument:
					PopArgument(index);
					break;
				case Segment.Constant:
					Console.WriteLine("A value can't be popped into the constant segment");
					break;
				case Segment.Local:
					PopLocal(index);
					break;
				case Segment.Pointer:
					PopPointer(index);
					break;
				case Segment.Static:
					PopStatic(index);
					break;
				case Segment.Temp:
					PopTemp(index);
					break;
				case Segment.That:
					PopThat(index);
					break;
				case Segment.This:
					PopThis(index);
					break;
			}
		}

		/// <summary>
		/// Pops the value into the given <paramref name="index"/> in the "Argument" segment.
		/// </summary>
		/// <param name="index">Index of the memory address in the segment</param>
		private void PopArgument(int index)
		{
			Pop("ARG", index);
		}

		/// <summary>
		/// Pops the value into the given <paramref name="index"/> in the "Local" segment.
		/// </summary>
		/// <param name="index">Index of the memory address in the segment</param>
		private void PopLocal(int index)
		{
			Pop("LCL", index);
		}

		/// <summary>
		/// Pops the value into the given <paramref name="index"/> in the "This" segment.
		/// </summary>
		/// <param name="index">Index of the memory address in the segment</param>
		private void PopThis(int index)
		{
			Pop("THIS", index);
		}

		/// <summary>
		/// Pops the value into the given <paramref name="index"/> in the "That" segment.
		/// </summary>
		/// <param name="index">Index of the memory address in the segment</param>
		private void PopThat(int index)
		{
			Pop("THAT", index);
		}

		/// <summary>
		/// Pops the value into the given <paramref name="index"/> in the "Pointer" segment.
		/// </summary>
		/// <param name="index">Index of the memory address in the segment</param>
		private void PopPointer(int index)
		{
			// Select memory
			writer.WriteLine("@R" + (3 + index));
			writer.WriteLine("D=A");
			
			writer.WriteLine("@tmp");
			writer.WriteLine("M=D");

			// Pop value off the stack
			writer.WriteLine("@SP");
			writer.WriteLine("A=M-1");
			writer.WriteLine("D=M");

			writer.WriteLine("@tmp");
			writer.WriteLine("A=M");
			writer.WriteLine("M=D");

			// Decrement SP
			writer.WriteLine("@SP");
			writer.WriteLine("M=M-1");
		}

		/// <summary>
		/// Pops the value into the given <paramref name="index"/> in the "Temp" segment.
		/// </summary>
		/// <param name="index">Index of the memory address in the segment</param>
		private void PopTemp(int index)
		{
			// Select memory
			writer.WriteLine("@R" + (5 + index));
			writer.WriteLine("D=A");

			writer.WriteLine("@tmp");
			writer.WriteLine("M=D");

			// Pop value off the stack
			writer.WriteLine("@SP");
			writer.WriteLine("A=M-1");
			writer.WriteLine("D=M");

			writer.WriteLine("@tmp");
			writer.WriteLine("A=M");
			writer.WriteLine("M=D");

			// Decrement SP
			writer.WriteLine("@SP");
			writer.WriteLine("M=M-1");
		}

		/// <summary>
		/// Pops the value into the given <paramref name="index"/> in the "Static" segment.
		/// </summary>
		/// <param name="index">Index of the memory address in the segment</param>
		private void PopStatic(int index)
		{
			// Select memory
			writer.WriteLine("@" + name + "." + index);
			writer.WriteLine("D=A");

			writer.WriteLine("@tmp");
			writer.WriteLine("M=D");

			// Pop value off the stack
			writer.WriteLine("@SP");
			writer.WriteLine("A=M-1");
			writer.WriteLine("D=M");

			writer.WriteLine("@tmp");
			writer.WriteLine("A=M");
			writer.WriteLine("M=D");

			// Decrement SP
			writer.WriteLine("@SP");
			writer.WriteLine("M=M-1");
		}

		/// <summary>
		/// Pops a value into the given <paramref name="index"/> in a <paramref name="segment"/>.
		/// </summary>
		/// <param name="index">Index of the memory address in the segment</param>
		private void Pop(string segment, int index)
		{
			// Select memory
			writer.WriteLine("@" + segment);
			writer.WriteLine("D=M");
			writer.WriteLine("@" + index);
			writer.WriteLine("D=D+A");

			writer.WriteLine("@tmp");
			writer.WriteLine("M=D");

			// Pop value off the stack
			writer.WriteLine("@SP");
			writer.WriteLine("A=M-1");
			writer.WriteLine("D=M");

			writer.WriteLine("@tmp");
			writer.WriteLine("A=M");
			writer.WriteLine("M=D");

			// Decrement SP
			writer.WriteLine("@SP");
			writer.WriteLine("M=M-1");
		}

		/// <summary>
		/// Pops a value into a variable
		/// </summary>
		/// <param name="var">Name of the variable</param>
		private void PopToVar(string var)
		{
			// Pop value off the stack
			writer.WriteLine("@SP");
			writer.WriteLine("A=M-1");
			writer.WriteLine("D=M");

			writer.WriteLine("@" + var);
			writer.WriteLine("M=D");

			// Decrement SP
			writer.WriteLine("@SP");
			writer.WriteLine("M=M-1");
		}

		#endregion

		/// <summary>
		/// Closes the <see cref="writer"/> writer
		/// </summary>
		public void Close()
		{
			if (writer != null)
			{
				writer.Close();
			}
		}
	}
}
