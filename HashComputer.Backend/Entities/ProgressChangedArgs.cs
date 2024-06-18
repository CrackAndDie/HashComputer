namespace HashComputer.Backend.Entities
{
	public class ProgressChangedArgs
	{
		/// <summary>
		/// Global progress of all threads
		/// </summary>
		public int Progress { get; set; }
		/// <summary>
		/// The thread number that passed the args
		/// </summary>
		public int ThreadNumber { get; set; }
		/// <summary>
		/// The current executed file
		/// </summary>
		public string Message { get; set; }
	}
}
