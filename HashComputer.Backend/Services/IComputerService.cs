namespace HashComputer.Backend.Services
{
	public interface IComputerService
	{
		(bool, string) ComputeHash(ComputeParameters parameters);
	}
}
