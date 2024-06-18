namespace HashComputer.Backend.Entities
{
	public class ComputedHashJson
	{
		public string Version { get; set; }
		/// <summary>
		/// In bytes
		/// </summary>
		public ulong TotalSize { get; set; }
		public Dictionary<string, string> ComputedHashes { get; set; }
	}
}
