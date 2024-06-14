using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Hypocrite.Core.Interfaces;
using Hypocrite.Core.Interfaces.Presentation;
using Hypocrite.Core.Logging.Interfaces;
using Hypocrite.Core.Logging.Services;
using Hypocrite.Core.Services;
using Hypocrite.Core.Utils.Settings;
using Hypocrite.Mvvm;
using Hypocrite.Services;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace HashComputer
{
	public partial class App : ApplicationBase
	{
		private static readonly string logFileFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoftHubSettings");
		private static readonly string logFileName = "HashComputer.log";

		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);

			base.Initialize();              // <-- Required
		}

		protected override AvaloniaObject CreateShell()
		{
			var viewModelService = Container.Resolve<IViewModelResolverService>();
			viewModelService.RegisterViewModelAssembly(Assembly.GetExecutingAssembly());

			return base.CreateShell();
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{
			base.RegisterTypes(containerRegistry);

			CheckForLogPathExistance();
			containerRegistry.RegisterInstance<ILoggingService>(new Log4netLoggingService(Path.Combine(logFileFolder, logFileName)));
			containerRegistry.RegisterSingleton<IViewModelResolverService, ViewModelResolverService>();
			containerRegistry.RegisterSingleton<IWindowProgressService, WindowProgressService>();

			containerRegistry.RegisterSingleton<IBaseWindow, MainWindow>();
		}

		private void CheckForLogPathExistance()
		{
			if (!Directory.Exists(logFileFolder))
			{
				Directory.CreateDirectory(logFileFolder);
			}
		}
	}
}