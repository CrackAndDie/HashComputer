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
			IsComputeButtonEnabled = false;

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
			ErrorText = result.Item2;

			IsProgressVisible = false;
			IsComputeButtonEnabled = true;
		}

		[Injection]
		IComputerService ComputerService { get; set; }

		[Notify]
		public string FolderPath { get; set; }
		[Notify]
		public ICommand ComputeHashCommand { get; set; }
		[Notify]
		public bool IsComputeButtonEnabled { get; set; } = true;

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
		public string ErrorText { get; set; }
	}
}
