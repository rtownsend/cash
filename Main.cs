using System;

using System.Collections.Generic;

namespace cash
{
	class MainClass
	{
		public static bool NotExiting = true;
		public static string WorkingDirectory = "~";
		public static string HostName = "";
		public static string UserName = "";
		public static void Main (string[] args)
		{	
			// Determine the build date. Thanks to John Leidegren on StackOverflow for this.
			var version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
			var buildDateTime = new DateTime(2000, 1, 1).Add(new TimeSpan(
			TimeSpan.TicksPerDay * version.Build + // days since 1 January 2000
			TimeSpan.TicksPerSecond * 2 * version.Revision)); // seconds since midnight, (multiply by 2 to get original)
			
			Console.WriteLine ("Hello World!");
			Console.WriteLine("This is cash, the C# shell.");
			Console.WriteLine("[Version Information: Version {0}, built on {1}]", "0.1", buildDateTime);
			
			HostName = Executive.ReturnBlockingTool("uname","-n");
			WorkingDirectory = Executive.ReturnBlockingTool("pwd","");
			UserName = Executive.ReturnBlockingTool("whoami","");
			
			while(NotExiting)
			{
				Console.Write("{0}@{1}:{2}# ",UserName,HostName,WorkingDirectory.Replace("/home/"+UserName.ToLower(),"~"));
				string input = Console.ReadLine().Trim();
	
				if(input.Length == 0) 
				{
					Console.WriteLine("ERROR: Cannot interpret a zero-length command!");
				}
				else InterpretAndRun(input);
				
			}
			
		}
		
		public static void Exit()
		{
			Console.WriteLine("Thank you for running cash.");
			Environment.Exit(0);
		}
				    
		public static void InterpretAndRun(string input)
		{
#if DEBUG
			Console.WriteLine("DEBUG: Entering InterpretAndRun()...");
#endif 
			List<Command> CommandQueue = new List<Command>();
			string[] parse1 = input.Split(new char[]{'|','&',';'});
			
			for(int i=0; i<parse1.Length; i++)
			{
#if DEBUG
					Console.WriteLine("DEBUG: parse1 array at position {0} is {1}.",i,parse1[i]);
#endif
				if(parse1[i].Length == 0)
				{
#if DEBUG
					Console.WriteLine("DEBUG: parse1 array at position {0} has zero length. Breaking.",i);
#endif
				}
				string filtered = input.Replace(" ","").Replace(
				                                                parse1[i].Replace(" ","")
				                                                ,"").Trim();
#if DEBUG
				Console.WriteLine("DEBUG: filtered string at parse1[{0}] is {1}.",i,filtered);
#endif
				if(filtered.Length == 0)
				{
					// We've got a simple command
#if DEBUG
					Console.WriteLine("DEBUG: {0} is a SimpleCommand.",parse1[i]);
#endif 
					CommandQueue.Add(new Command(CommandTypes.SimpleCommand,parse1[i]));
				}
				else if(filtered[0] == '|')
				{
					// Then we either have a pipeline or an or list
					if(filtered[1] == '|')
					{
						// Then it is an or list
						CommandQueue.Add(new Command(CommandTypes.NextInOrList,parse1[i]));
					}
					else 
					{
						// Then it is a pipeline
						CommandQueue.Add(new Command(CommandTypes.PipedCommand,parse1[i]));
					}
				}
				
				else if(filtered[0] == '&')
				{
					// Then we have probably got an and list
					CommandQueue.Add(new Command(CommandTypes.NextInAndList,parse1[i]));
				}
				
			}
			Executive.ExecuteCommandList(CommandQueue);
		}
	}
}
