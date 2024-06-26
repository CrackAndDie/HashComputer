using CommandLine;
using HashComputer.Backend;

namespace HashComputer.Cli
{
	public class HashOptions
	{
		[Option('v', Default = ComputeParameters.DEFAULT_VERSION, HelpText = "Specifies the version of a hash file")]
		public string Version { get; set; }

		[Option('d', Default = ".", HelpText = "The directory where to compute hash")]
		public string CheckDir { get; set; }

		[Option('t', Default = ComputeParameters.DEFAULT_TASK_NUMBER, HelpText = "The amount of tasks to be used to calc hash")]
		public int TaskAmount { get; set; }

		[Option('o', Default = ComputeParameters.DEFAULT_HASH_FILENAME, HelpText = "The name of the hash file. (Only relative path is supported)")]
		public string OutDir { get; set; }
	}
}
