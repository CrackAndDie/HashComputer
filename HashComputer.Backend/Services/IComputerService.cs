﻿using HashComputer.Backend.Entities;

namespace HashComputer.Backend.Services
{
	public interface IComputerService
	{
		/// <summary>
		/// Computes hash and generates file
		/// </summary>
		/// <param name="parameters">Compute parameters</param>
		/// <param name="onProgressChanged">Called when progress changed (in percents)</param>
		/// <returns><see cref="true"/> - on success generation overwise - <see cref="false"/>. 
		/// The second parameter is used to describe the failure or the files diff on success.</returns>
		Task<(bool, string)> ComputeHash(ComputeParameters parameters, Action<ProgressChangedArgs> onProgressChanged = null, CancellationToken cancellationToken = default);

		/// <summary>
		/// Computes hash
		/// </summary>
		/// <param name="parameters">Compute parameters</param>
		/// <param name="onProgressChanged">Called when progress changed (in percents)</param>
		/// <returns><see cref="true"/> - on success generation overwise - <see cref="false"/>. 
		/// The second parameter is used to describe the failure or the files diff on success.</returns>
		Task<ComputedHashJson> ComputeHashPure(ComputeParameters parameters, Action<ProgressChangedArgs> onProgressChanged = null, CancellationToken cancellationToken = default);
	}
}
