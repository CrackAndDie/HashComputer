using CommandLine;

namespace HashComputer.Cli
{
	public class HashOptions
	{
		[Option('v', "version", HelpText = "Specifies the version of a hash file", Default = "1.0.0")]
		public string Version { get; set; }

		[Option('d', "directory", Default = ".", HelpText = "The directory to check")]
		public string CheckDir { get; set; }
	}
}
