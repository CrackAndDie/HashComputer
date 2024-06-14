namespace HashComputer.Backend
{
	/// <summary>
	/// The parameters that has to be passed to the backend
	/// </summary>
	public class ComputeParameters
	{
		public const string DEFAULT_HASH_FILENAME = "computed_hash";

		/// <summary>
		/// Path to the folder where the hash should be computed
		/// </summary>
		public string Path { get; set; }
		/// <summary>
		/// The version of the files. If <see cref="null"/> then the version is not included
		/// </summary>
		public string Version { get; set; }
		/// <summary>
		/// The filename to use for the output hash file. If <see cref="null"/> then <see cref="DEFAULT_HASH_FILENAME"/> is used
		/// </summary>
		public string HashFileName { get; set; }
	}
}
