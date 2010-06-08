using System;

#if NET_COMPATABLE
using System.Diagnostics;
using System.IO;

using Mono.Unix;

#endif 

namespace cash
{
	
	/// <summary>
	/// Used to represent the possible ways of chaining together 
	/// commands.
	/// </summary>
	public enum CommandTypes
	{
		SimpleCommand,
		PipedCommand,
		NextInAndList,
		NextInOrList,
		InputList,
		ArithmeticExpression
	}
	
	public class Command
	{
		public CommandTypes CommandType {set;get;}
		public string CommandText {set;get;}
		public string Arguments{set;get;}
		public string StdIn{set;get;}
		public string StdOut{set;get;}
		public string StdErr{set;get;}
		public int ExitCode{set;get;}
		
		/// <summary>
		/// Create a new command with type Type and path (with arguments) Cmd.
		/// </summary>
		/// <param name="Type">
		/// The enumeration representing what the command's purpose is <see cref="CommandTypes"/>
		/// </param>
		/// <param name="Cmd">
		/// The command (plus arguments) used. <see cref="System.String"/>
		/// </param>
		public Command(CommandTypes Type, string Cmd)
		{
			string[] Input = Cmd.Split(' ');
			CommandText = Input[0].Trim();
			
			if(Input.Length > 1)
			{
				Arguments = "";
				for(int i=1; i<Input.Length-1; i++)
				{
					Arguments+= Input[i] + " ";
				}
				Arguments += Input[Input.Length-1];
			}
			
			CommandType = Type;
		}
		
		public void Execute()
		{
			
			if(CommandText == "cd")
			{
#if DEBUG
				Console.WriteLine("DEBUG: changing directory to {0}...",Arguments);
#endif 
				string PreviousDirectory = MainClass.WorkingDirectory;
				int result = Mono.Unix.Native.Syscall.chdir(Arguments);
				MainClass.WorkingDirectory = Executive.ReturnBlockingTool("pwd",""); //Not strictly necassary, I'm sure.
				
				if(result == -1)
				{
					Console.WriteLine("cash: cd: {0}: Directory not changed (permission denied.)",Arguments);
				}
				
				return;
			}
			
#if NET_COMPATABLE
#if DEBUG
			Console.WriteLine("DEBUG: Starting \"{0}\" with arguments \"{1}\"...",CommandText,Arguments);
#endif 
			ProcessStartInfo ps = new ProcessStartInfo(CommandText,Arguments);
			ps.RedirectStandardError = true;
			ps.RedirectStandardInput = true;
			ps.RedirectStandardOutput = true;
			ps.UseShellExecute = false;
			
			Process process = Process.Start(ps);
			StreamWriter StdInput = process.StandardInput;
			StdInput.Write(StdIn);
			process.Start();
			process.WaitForExit();
			StdOut = process.StandardOutput.ReadToEnd();
			StdErr = process.StandardOutput.ReadToEnd();
			ExitCode = process.ExitCode;
			#if DEBUG
			Console.WriteLine("DEBUG: Command \"{0}\" exited with code {1}.",CommandText,ExitCode);
			Console.WriteLine("DEBUG: StdOut was \"{0}\" ",StdOut);
			Console.WriteLine("DEBUG: StdIn was \"{0}\" ",StdIn);
			Console.WriteLine("DEBUG: StdErr was \"{0}\" ",StdErr);
			
			
			
	#endif 
#endif 
		}
	}
}
