using Avalonia.Threading;
using HashComputer.Backend.Services;
using Hypocrite.Core.Container;
using Hypocrite.Core.Mvvm.Attributes;
using Hypocrite.Mvvm;
using Prism.Commands;
using System.Windows.Input;

namespace HashComputer
{
	public class MainWindowViewModel : ViewModelBase
	{
		public MainWindowViewModel()
		{
			ComputeHashCommand = new DelegateCommand(OnComputeHashCommand);
		}

		private async void OnComputeHashCommand()
		{
			IsDoneVisible = false;
			IsFailureVisible = false;

			IsProgressVisible = true;
			IsInteractionEnabled = false;

			DiffText = string.Empty; // reset

			var result = await ComputerService.ComputeHash(
				new Backend.ComputeParameters()
				{
					Path = FolderPath,
				}, 
				(val, filename) =>
				{
					Dispatcher.UIThread.Invoke(() =>
					{
						CurrentProgress = val;
						CurrentFile = filename;
					});
				}
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

		[Injection]
		IComputerService ComputerService { get; set; }

		[Notify]
		public string FolderPath { get; set; }
		[Notify]
		public ICommand ComputeHashCommand { get; set; }
		[Notify]
		public bool IsInteractionEnabled { get; set; } = true;

		[Notify]
		public int CurrentProgress { get; set; }
		[Notify]
		public string CurrentFile { get; set; }
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
