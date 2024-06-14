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

		private void OnComputeHashCommand()
		{
			ComputerService.ComputeHash(new Backend.ComputeParameters()
			{
				Path = FolderPath,
			});
		}

		[Injection]
		IComputerService ComputerService { get; set; }

		[Notify]
		public string FolderPath { get; set; }
		[Notify]
		public ICommand ComputeHashCommand { get; set; }
	}
}
