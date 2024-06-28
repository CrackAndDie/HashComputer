using HashComputer.Backend.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;

namespace HashComputer.Backend.Services
{
	public class ComputerService : IComputerService
	{
		/// <inheritdoc/>
		public async Task<(bool, string)> ComputeHash(ComputeParameters parameters, Action<ProgressChangedArgs> onProgressChanged = null, CancellationToken cancellationToken = default)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(parameters.Path))
					return (false, "Path was empty");

				if (!Directory.Exists(parameters.Path))
					return (false, "Directory does not exist or there is a typo in it");

				var computedHashJson = await ComputeHashPure(parameters, onProgressChanged, cancellationToken);

				string fileName = parameters.HashFileName ?? ComputeParameters.DEFAULT_HASH_FILENAME;
				string filePath = $"{parameters.Path.Trim('/').Trim('\\')}/{fileName}.json";

				string diffText = string.Empty;
				if (File.Exists(filePath))
				{
					var prevJson = JsonConvert.DeserializeObject<ComputedHashJson>(await File.ReadAllTextAsync(filePath));
					StringBuilder sb = new StringBuilder();
					foreach (var pair in computedHashJson.ComputedHashes)
					{
						if (prevJson.ComputedHashes.ContainsKey(pair.Key) && prevJson.ComputedHashes[pair.Key] == pair.Value)
							continue;
						sb.AppendLine(pair.Key);
					}
					diffText = sb.ToString();
				}

				string data = JsonConvert.SerializeObject(computedHashJson);
				await File.WriteAllTextAsync(filePath, data, cancellationToken);

				onProgressChanged?.Invoke(new ProgressChangedArgs()
				{
					Progress = 100,
					ThreadNumber = 0,
					Message = string.Empty,
				});

				return (true, diffText);
			}
			catch (Exception ex)
			{
				return (false, "Unhandled exception: \n" + ex.ToString());
			}
		}

		/// <inheritdoc/>
		public async Task<ComputedHashJson> ComputeHashPure(ComputeParameters parameters, Action<ProgressChangedArgs> onProgressChanged = null, CancellationToken cancellationToken = default)
		{
			parameters.Path = parameters.Path.Replace("\\", "/");

			// TODO: cringe - rewrite
			string localStableFile = SearchForStableFile(parameters.Path);
			List<string> stableFiles = new List<string>();
			if (!string.IsNullOrWhiteSpace(parameters.StableFilesPath) && File.Exists(parameters.StableFilesPath))
			{
				var lines = File.ReadAllText(parameters.StableFilesPath).Split(Environment.NewLine).Select(x => x.Trim());
				lines = lines.Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith('#')); // skip empty and comments
				stableFiles.AddRange(lines);
			}
			else if (!string.IsNullOrWhiteSpace(localStableFile) && File.Exists(localStableFile))
			{
				var lines = File.ReadAllText(localStableFile).Split(Environment.NewLine).Select(x => x.Trim());
				lines = lines.Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith('#')); // skip empty and comments
				stableFiles.AddRange(lines);
			}

			int taskNumber = parameters.TaskNumber <= 0 ? ComputeParameters.DEFAULT_TASK_NUMBER : parameters.TaskNumber;
			var data = await GetFileHashMappings(parameters.Path, taskNumber, stableFiles, onProgressChanged, cancellationToken);

			ComputedHashJson computedHashJson = new ComputedHashJson()
			{
				Version = parameters.Version ?? ComputeParameters.DEFAULT_VERSION,
				ComputedHashes = data.Item1,
				TotalSize = data.Item2,
			};
			return computedHashJson;
		}

		/// <summary>
		/// Returns all the file names (full path) from dicrectory and subdirectories
		/// </summary>
		/// <param name="folderPath">The folder path</param>
		/// <returns>File names</returns>
		private IEnumerable<string> GetAllFiles(string folderPath)
		{
			//return Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
			return Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories);
		}

		private string SearchForStableFile(string folderPath)
		{
			string dir = folderPath;
			bool found = false;
			while (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir) && !found)
			{
				dir = dir.Trim('/').Trim('\\');
				string filePath = $"{dir}/{ComputeParameters.DEFAULT_STABLE_FILENAME}.txt";
				if (File.Exists(filePath))
				{
					return filePath;
				}
				dir = Directory.GetParent(dir.EndsWith("/") ? dir : string.Concat(dir, "/"))?.Parent?.FullName;
			}
			return string.Empty;
		}

		/// <summary>
		/// Reads files, computes its hash and returns it
		/// </summary>
		/// <param name="filePath">The file name</param>
		/// <returns>Computed hash</returns>
		private async Task<string> GetFileHash(string filePath, CancellationToken cancellationToken = default)
		{
			using FileStream stream = File.OpenRead(filePath);
			using SHA512 sha = SHA512.Create();
			byte[] hash = await sha.ComputeHashAsync(stream, cancellationToken);
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
		private async Task<(Dictionary<string, string>, ulong)> GetFileHashMappings(string folderPath, int taskNumber, List<string> stableFiles, Action<ProgressChangedArgs> onProgressChanged = null, CancellationToken cancellationToken = default)
		{
			ulong totalSize = 0;
			Dictionary<string, string> result = new Dictionary<string, string>();
			object resultLock = new object();

			var allFiles = GetAllFiles(folderPath);
			var filesQueue = new ConcurrentQueue<string>(allFiles);

			var len = await Task.Run<int>(() => { return allFiles.Count(); });

			int currentFileIndex = 0;
			object currentFileIndexLock = new object();

			List<Task> tasksToAwait = new List<Task>();
			// an amount of tasks 
			for (int i = 0; i < taskNumber; ++i)
			{
				tasksToAwait.Add(FileProcessor(i + 1, cancellationToken));
			}

			await Task.WhenAll(tasksToAwait);

			return (result, totalSize);

			Task FileProcessor(int number, CancellationToken cancellationToken = default)
			{
				return Task.Run(async () =>
				{
					while (filesQueue.TryDequeue(out var file))
					{
						if (cancellationToken.IsCancellationRequested)
							break;

						string normalized = file.Replace("\\", "/");
						string lowerName = GetLowerFileName(folderPath, normalized);

						lock (currentFileIndexLock)
						{
							currentFileIndex++;
							onProgressChanged?.Invoke(new ProgressChangedArgs()
							{
								Progress = (int)(currentFileIndex / (float)len * 100),
								ThreadNumber = number,
								Message = lowerName,
							});
						}

						string fileHash;
						// if it is a stable file
						if (stableFiles.Contains(lowerName))
						{
							fileHash = "--stable--";
						}
						else
						{
							fileHash = await GetFileHash(normalized, cancellationToken);
						}
						
						var fileSize = new System.IO.FileInfo(normalized).Length;

						lock (resultLock)
						{
							result.Add(lowerName, fileHash);
							totalSize += (ulong)fileSize;
						}
					}
					lock (currentFileIndexLock)
					{
						onProgressChanged?.Invoke(new ProgressChangedArgs()
						{
							Progress = (int)(currentFileIndex / (float)len * 100),
							ThreadNumber = number,
							Message = string.Empty,
						});
					}
				});
			}
		}
	}
}
