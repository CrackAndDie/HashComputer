namespace HashComputer.Backend.Services
{
	public interface IComputerService
	{
		/// <summary>
		/// Computes hash and generates file
		/// </summary>
		/// <param name="parameters">Compute parameters</param>
		/// <returns><see cref="true"/> - on success generation overwise - <see cref="false"/>. 
		/// The second parameter is used to describe the failure.</returns>
		Task<(bool, string)> ComputeHash(ComputeParameters parameters);
	}
}
