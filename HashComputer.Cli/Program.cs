using CommandLine;
using HashComputer.Backend;
using HashComputer.Backend.Entities;
using HashComputer.Backend.Services;

namespace HashComputer.Cli
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCancelCommand);

			var argsParser = Parser.Default;
			var taskToWait = argsParser.ParseArguments<HashOptions>(args).MapResult<HashOptions, Task>(RunHasher, (_) =>
			{
				return Task.CompletedTask;
			});
			taskToWait.GetAwaiter().GetResult();
		}

		private static Task RunHasher(HashOptions options)
		{
            _currentOptions = options;
            return RunHasherInternal(options);
		}

		private async static Task RunHasherInternal(HashOptions options)
		{
			if (!options.MuteHashComputer)
				Console.WriteLine("Begin computing hash...");

			_startCursorPos = Console.GetCursorPosition();

			var computerService = new ComputerService();
			_currentCancellationToken = new CancellationTokenSource();
			_currentTaskAmount = options.TaskAmount;

			var result = await computerService.ComputeHash(
				new Backend.ComputeParameters()
				{
					Path = options.CheckDir,
					Version = options.Version,
					TaskNumber = options.TaskAmount,
					HashFileName = options.OutDir,
					StableFilesPath = options.StableFilePath,
				},
				OnProgressChanged,
				_currentCancellationToken.Token
			);

			OnExit(result.Item1);

			if (result.Item1 && !string.IsNullOrWhiteSpace(result.Item2))
			{
                if (!options.MuteHashComputer)
                    Console.WriteLine("There was already a hash file. Here are the files that changed or not presented yet:\n");
			}
            if (!options.MuteHashComputer)
                Console.WriteLine(result.Item2);
		}

		private static void OnProgressChanged(ProgressChangedArgs args)
		{
            if (!_currentOptions.MuteHashComputer)
			{
                UpdateTextOnLine(0, args.Progress.ToString() + "%");
                if (args.ThreadNumber > 0)
                {
                    UpdateTextOnLine(args.ThreadNumber, $"Task {args.ThreadNumber}: {args.Message}");
                }
            }
		}

		private static void UpdateTextOnLine(int lineNumber, string text)
		{
			Console.SetCursorPosition(_startCursorPos.Item1, _startCursorPos.Item2 + lineNumber);
			if (text.Length > Console.BufferWidth)
			{
				int diff = text.Length - Console.BufferWidth;
				text = text.Substring(0, text.Length - diff);
			}
			else
			{
				text = text + new String(' ', Console.BufferWidth - text.Length);
			}
			Console.WriteLine(text);
		}

		private static void OnCancelCommand(object sender, ConsoleCancelEventArgs args)
		{
			_currentCancellationToken?.Cancel();
			OnExit(false);
		}

		private static void OnExit(bool properly)
		{
            if (!_currentOptions.MuteHashComputer)
			{
                // + 1 is for percents
                for (int i = 0; i < _currentTaskAmount + 1; ++i)
                {
                    Console.SetCursorPosition(_startCursorPos.Item1, _startCursorPos.Item2 + i);
                    Console.Write(new String(' ', Console.BufferWidth));
                }
                Console.SetCursorPosition(_startCursorPos.Item1, _startCursorPos.Item2);
            }
			Console.WriteLine(properly ? "Done computing hash..." : "Error while computing hash...");
		}

		private static HashOptions _currentOptions;
		private static int _currentTaskAmount = ComputeParameters.DEFAULT_TASK_NUMBER;
		private static CancellationTokenSource _currentCancellationToken;
		private static (int, int) _startCursorPos;
	}
}
