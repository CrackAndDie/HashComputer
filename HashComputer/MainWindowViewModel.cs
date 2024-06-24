using Avalonia.Threading;
using HashComputer.Backend.Services;
using Hypocrite.Core.Container;
using Hypocrite.Core.Mvvm.Attributes;
using Hypocrite.Mvvm;
using Prism.Commands;
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
			IsDoneVisible = false;
			IsFailureVisible = false;

			IsProgressVisible = true;
			IsInteractionEnabled = false;

			DiffText = string.Empty; // reset

			_currentCancellationToken = new CancellationTokenSource();
			var result = await ComputerService.ComputeHash(
				new Backend.ComputeParameters()
				{
					Path = FolderPath,
					Version = VersionText,
				}, 
				(val) =>
				{
					Dispatcher.UIThread.Invoke(() =>
					{
						CurrentProgress = val.Progress;
						switch (val.ThreadNumber)
						{
							// TODO: as listview
							case 1: CurrentFile1 = val.Message; break;
							case 2: CurrentFile2 = val.Message; break;
							case 3: CurrentFile3 = val.Message; break;
							case 4: CurrentFile4 = val.Message; break;
						}
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
		public string VersionText { get; set; } = "1.0.0";
		[Notify]
		public ICommand ComputeHashCommand { get; set; }
		[Notify]
		public ICommand CancelCommand { get; set; }
		[Notify]
		public bool IsInteractionEnabled { get; set; } = true;

		[Notify]
		public int CurrentProgress { get; set; }
		[Notify]
		public string CurrentFile1 { get; set; }
		[Notify]
		public string CurrentFile2 { get; set; }
		[Notify]
		public string CurrentFile3 { get; set; }
		[Notify]
		public string CurrentFile4 { get; set; }
		[Notify]
		public bool IsProgressVisible { get; set; }

		[Notify]
		public bool IsDoneVisible { get; set; }
		[Notify]
		public bool IsFailureVisible { get; set; }
		[Notify]
		public string DiffText { get; set; }
	}
}
