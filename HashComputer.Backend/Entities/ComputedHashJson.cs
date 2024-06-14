namespace HashComputer.Backend.Entities
{
	public class ComputedHashJson
	{
		public string Version { get; set; }
		public Dictionary<string, string> ComputedHashes { get; set; }
	}
}
