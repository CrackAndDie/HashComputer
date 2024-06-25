using CommandLine;
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
			return RunHasherInternal(options);
		}

		private async static Task RunHasherInternal(HashOptions options)
		{
			Console.WriteLine("Begin computing hash...");

			_startCursorPos = Console.GetCursorPosition();

			var computerService = new ComputerService();
			_currentCancellationToken = new CancellationTokenSource();

			var result = await computerService.ComputeHash(
				new Backend.ComputeParameters()
				{
					Path = options.CheckDir,
					Version = options.Version,
				},
				OnProgressChanged,
				_currentCancellationToken.Token
			);

			OnExit(result.Item1);

			if (result.Item1 && !string.IsNullOrWhiteSpace(result.Item2))
			{
				Console.WriteLine("There was already a hash file. Here are the files that changed or not presented yet:\n");
			}
			Console.WriteLine(result.Item2);
		}

		private static void OnProgressChanged(ProgressChangedArgs args)
		{
			UpdateTextOnLine(0, args.Progress.ToString() + "%");
			switch (args.ThreadNumber)
			{
				// TODO: multi shite
				case 1: UpdateTextOnLine(1, "Thread 1: " + args.Message); break;
				case 2: UpdateTextOnLine(2, "Thread 2: " + args.Message); break;
				case 3: UpdateTextOnLine(3, "Thread 3: " + args.Message); break;
				case 4: UpdateTextOnLine(4, "Thread 4: " + args.Message); break;
			}
		}

		// 0 - perc, 1-4 - curr files
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
			Thread.Sleep(10);
		}

		private static void OnCancelCommand(object sender, ConsoleCancelEventArgs args)
		{
			_currentCancellationToken?.Cancel();
			OnExit(false);
		}

		private static void OnExit(bool properly)
		{
			for (int i = 0; i < 5; ++i)
			{
				Console.SetCursorPosition(_startCursorPos.Item1, _startCursorPos.Item2 + i);
				Console.Write(new String(' ', Console.BufferWidth));
			}
			Console.SetCursorPosition(_startCursorPos.Item1, _startCursorPos.Item2);
			Console.WriteLine(properly ? "Done..." : "Error...");
		}

		private static CancellationTokenSource _currentCancellationToken;
		private static (int, int) _startCursorPos;
	}
}
