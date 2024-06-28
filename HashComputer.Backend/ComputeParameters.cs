namespace HashComputer.Backend
{
	/// <summary>
	/// The parameters that has to be passed to the backend
	/// </summary>
	public class ComputeParameters
	{
		public const string DEFAULT_HASH_FILENAME = "computed_hash";
		public const string DEFAULT_STABLE_FILENAME = "computed_stables"; // not really computed but this is for placing near hash
		public const string DEFAULT_VERSION = "1.0.0";
		public const int DEFAULT_TASK_NUMBER = 4;

		/// <summary>
		/// Path to the folder where the hash should be computed
		/// </summary>
		public string Path { get; set; }
		/// <summary>
		/// The version of the files. If <see cref="null"/> then the version is <see cref="DEFAULT_VERSION"/>
		/// </summary>
		public string Version { get; set; }
		/// <summary>
		/// The filename to use for the output hash file. If <see cref="null"/> then <see cref="DEFAULT_HASH_FILENAME"/> is used
		/// </summary>
		public string HashFileName { get; set; }

		/// <summary>
		/// Task amount used to calc hash
		/// </summary>
		public int TaskNumber { get; set; }

		/// <summary>
		/// Path to the stable files
		/// </summary>
		public string StableFilesPath { get; set; }
	}
}
