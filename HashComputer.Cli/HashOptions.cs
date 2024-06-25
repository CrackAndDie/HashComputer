using CommandLine;

namespace HashComputer.Cli
{
	public class HashOptions
	{
		[Option('v', HelpText = "Specifies the version of a hash file", Default = "1.0.0")]
		public string Version { get; set; }

		[Option('d', Default = ".", HelpText = "The directory where to compute hash")]
		public string CheckDir { get; set; }
	}
}
