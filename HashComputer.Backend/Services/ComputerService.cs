using HashComputer.Backend.Entities;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace HashComputer.Backend.Services
{
	public class ComputerService : IComputerService
	{
		public async Task<(bool, string)> ComputeHash(ComputeParameters parameters, Action<int, string> onProgressChanged = null)
		{
			if (string.IsNullOrWhiteSpace(parameters.Path))
				return (false, "Path was empty");

			if (!Directory.Exists(parameters.Path))
				return (false, "Directory does not exist or there is a typo in it");

			parameters.Path = parameters.Path.Replace("\\", "/");

			ComputedHashJson computedHashJson = new ComputedHashJson()
			{
				Version = parameters.Version ?? ComputeParameters.DEFAULT_VERSION,
				ComputedHashes = await GetFileHashMappings(parameters.Path, onProgressChanged),
			};

			string fileName = parameters.HashFileName ?? ComputeParameters.DEFAULT_HASH_FILENAME;
			string filePath = $"{parameters.Path.Trim('/')}/{fileName}.json";

			string diffText = string.Empty;
			if (File.Exists(filePath))
			{
				var prevJson = JsonConvert.DeserializeObject<ComputedHashJson>(await File.ReadAllTextAsync(filePath));
				StringBuilder sb = new StringBuilder();
                foreach (var pair in prevJson.ComputedHashes)
                {
					if (computedHashJson.ComputedHashes.ContainsKey(pair.Key) && computedHashJson.ComputedHashes[pair.Key] == pair.Value)
						continue;
					sb.AppendLine(pair.Key);
				}
				diffText = sb.ToString();
			}

			string data = JsonConvert.SerializeObject(computedHashJson);
			await File.WriteAllTextAsync(filePath, data);

			onProgressChanged?.Invoke(100, string.Empty);

			return (true, diffText);
		}

		/// <summary>
		/// Returns all the file names (full path) from dicrectory and subdirectories
		/// </summary>
		/// <param name="folderPath">The folder path</param>
		/// <returns>File names</returns>
		private string[] GetAllFiles(string folderPath)
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
		/// <param name="onProgressChanged">Called when progress changed</param>
		/// <returns>Mappings</returns>
		private async Task<Dictionary<string, string>> GetFileHashMappings(string folderPath, Action<int, string> onProgressChanged = null)
		{
			Dictionary<string, string> result = new Dictionary<string, string>();
			var allFiles = GetAllFiles(folderPath);
			var len = allFiles.Length;
			for (int i = 0; i < len; ++i)
			{
				string normalized = allFiles[i].Replace("\\", "/");

				onProgressChanged?.Invoke((int)(i / (float)len * 100), normalized);
				
				string fileHash = await GetFileHash(normalized);
				string lowerName = GetLowerFileName(folderPath, normalized);
				result.Add(lowerName, fileHash);
			}
			return result;
		}
	}
}
