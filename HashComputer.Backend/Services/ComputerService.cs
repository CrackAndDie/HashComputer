using HashComputer.Backend.Entities;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace HashComputer.Backend.Services
{
	public class ComputerService : IComputerService
	{
		public async Task<(bool, string)> ComputeHash(ComputeParameters parameters)
		{
			parameters.Path = parameters.Path.Replace("\\", "/");

			ComputedHashJson computedHashJson = new ComputedHashJson()
			{
				Version = parameters.Version ?? ComputeParameters.DEFAULT_VERSION,
				ComputedHashes = await GetFileHashMappings(parameters.Path),
			};

			string fileName = parameters.HashFileName ?? ComputeParameters.DEFAULT_HASH_FILENAME;

			string data = JsonConvert.SerializeObject(computedHashJson);
			await File.WriteAllTextAsync($"{parameters.Path.Trim('/')}/{fileName}.json", data);

			return (true, string.Empty);
		}

		/// <summary>
		/// Returns all the file names (full path) from dicrectory and subdirectories
		/// </summary>
		/// <param name="folderPath">The folder path</param>
		/// <returns>File names</returns>
		private IEnumerable<string> GetAllFiles(string folderPath)
		{
			return Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
		}

		/// <summary>
		/// Reads files, computes its hash and returns it
		/// </summary>
		/// <param name="filePath">The file name</param>
		/// <returns>Computed hash</returns>
		private async Task<string> GetFileHash(string filePath)
		{
			using FileStream stream = File.OpenRead(filePath);
			using SHA512 sha = SHA512.Create();
			byte[] hash = await sha.ComputeHashAsync(stream);
			return BitConverter.ToString(hash).Replace("-", String.Empty);
		}

		/// <summary>
		/// Generates relative file path from an absolute
		/// </summary>
		/// <param name="folderPath">Folder path to be as an anchor</param>
		/// <param name="filePath">The file path</param>
		/// <returns>Relative path of the file</returns>
		private string GetLowerFileName(string folderPath, string filePath)
		{
			StringBuilder sb = new StringBuilder();
			int fpInd = 0;
			while (fpInd < filePath.Length)
			{
				if (fpInd >= folderPath.Length)
				{
					sb.Append(filePath[fpInd]);
				}
				++fpInd;
			}
			return sb.ToString().TrimStart('/');
		}

		/// <summary>
		/// Generates file name - file hash mappings
		/// </summary>
		/// <param name="folderPath">Path to the folder with files</param>
		/// <returns>Mappings</returns>
		private async Task<Dictionary<string, string>> GetFileHashMappings(string folderPath)
		{
			Dictionary<string, string> result = new Dictionary<string, string>();
			foreach (var file in GetAllFiles(folderPath))
			{
				string normalized = file.Replace("\\", "/");
				string fileHash = await GetFileHash(normalized);
				string lowerName = GetLowerFileName(folderPath, normalized);
				result.Add(lowerName, fileHash);
			}
			return result;
		}
	}
}
