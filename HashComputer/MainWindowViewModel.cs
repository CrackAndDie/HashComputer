using Avalonia.Threading;
using HashComputer.Backend;
using HashComputer.Backend.Services;
using Hypocrite.Core.Container;
using Hypocrite.Core.Mvvm.Attributes;
using Hypocrite.Mvvm;
using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;

namespace HashComputer
{
	public class MainWindowViewModel : ViewModelBase
	{
		public MainWindowViewModel()
		{
			ComputeHashCommand = new DelegateCommand(OnComputeHashCommand);
			CancelCommand = new DelegateCommand(OnCancelCommand);
		}

		private async void OnComputeHashCommand()
		{
			var parameters = new Backend.ComputeParameters()
			{
				Path = FolderPath,
				Version = VersionText,
				TaskNumber = TaskNumber,
			};

			IsDoneVisible = false;
			IsFailureVisible = false;

			IsProgressVisible = true;
			IsInteractionEnabled = false;

			lock (_currentFilesLock)
				CurrentFiles.AddRange(Enumerable.Repeat("", parameters.TaskNumber));

			DiffText = string.Empty; // reset

			_currentCancellationToken = new CancellationTokenSource();
			var result = await ComputerService.ComputeHash(
				parameters,
				(val) =>
				{
					Dispatcher.UIThread.Invoke(() =>
					{
						CurrentProgress = val.Progress;
						UpdateCurrentFileStatus(val.ThreadNumber, val.Message);
					});
				},
				_currentCancellationToken.Token
			);

			IsDoneVisible = result.Item1;
			IsFailureVisible = !result.Item1;

			bool isDiffTextVisible = !string.IsNullOrWhiteSpace(result.Item2) && result.Item1;

			// if done correctly and not empty - Item2 is diff 
			if (isDiffTextVisible)
				DiffText = "There was already a hash file. Here are the files that changed or not presented yet:\n";
			else if (IsDoneVisible)
				DiffText = "Done!"; // just write "Done" on success
			DiffText += result.Item2;

			IsProgressVisible = false;
			IsInteractionEnabled = true;
			lock (_currentFilesLock)
				CurrentFiles.Clear();
		}

		private void UpdateCurrentFileStatus(int num, string message)
		{
			lock (_currentFilesLock)
			{
				if (CurrentFiles.Count > 0 && num > 0)
					CurrentFiles[num - 1] = $"Task {num}: {message}";			
			}
		}

		private void OnCancelCommand()
		{
			_currentCancellationToken?.Cancel();
		}

		private CancellationTokenSource _currentCancellationToken;

		[Injection]
		IComputerService ComputerService { get; set; }

		[Notify]
		public string FolderPath { get; set; }
		[Notify]
		public string VersionText { get; set; } = ComputeParameters.DEFAULT_VERSION;
		[Notify]
		public int TaskNumber { get; set; } = ComputeParameters.DEFAULT_TASK_NUMBER;
		[Notify]
		public string OutFileName { get; set; } = ComputeParameters.DEFAULT_HASH_FILENAME;

		[Notify]
		public ICommand ComputeHashCommand { get; set; }
		[Notify]
		public ICommand CancelCommand { get; set; }
		[Notify]
		public bool IsInteractionEnabled { get; set; } = true;

		[Notify]
		public int CurrentProgress { get; set; }
		[Notify]
		public bool IsProgressVisible { get; set; }

		[Notify]
		public bool IsDoneVisible { get; set; }
		[Notify]
		public bool IsFailureVisible { get; set; }
		[Notify]
		public string DiffText { get; set; }

		private object _currentFilesLock = new object();
		public ObservableCollection<string> CurrentFiles { get; set; } = new ObservableCollection<string>();
	}
}
