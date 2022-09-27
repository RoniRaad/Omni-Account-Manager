using System.IO;
using System.Reflection;
using System.Windows;
using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models.AppSettings;
using AccountManager.Core.Services;
using AccountManager.Core.Services.GraphServices;
using AccountManager.Core.Services.GraphServices.Cached;
using AccountManager.Infrastructure.CachedClients;
using AccountManager.Infrastructure.Clients;
using AccountManager.Infrastructure.Services;
using AccountManager.Infrastructure.Services.FileSystem;
using AccountManager.Infrastructure.Services.Platform;
using AccountManager.Infrastructure.Services.Token;
using AccountManager.UI.Extensions;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NeoSmart.Caching.Sqlite;
using Plk.Blazor.DragDrop;
using Squirrel;
using Microsoft.Extensions.Options;
using AccountManager.Core.Models.UserSettings;
using System.Collections.Generic;
using System;
using AccountManager.Core.Models.RiotGames.Valorant;
using Microsoft.Extensions.Logging;
using System.Configuration;
using AccountManager.UI.Services;
using AccountManager.Core.Models.EpicGames;

namespace AccountManager.UI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
    {
		public IConfigurationRoot Configuration { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Vulnerability", 
			"S4830:Server certificates should be verified during SSL/TLS connections", Justification = "This is for communicating with a local api.")]
        public MainWindow()
        {
			var builder = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
			Configuration = builder.Build();

            GeneralFileSystemService.InitializeFileSystem();

            IServiceCollection services = new ServiceCollection();
            Startup.ConfigureServices(services, Configuration);

			var builtServiceProvider = services.BuildServiceProvider();

            Resources.Add("services", builtServiceProvider);

			InitializeComponent();
			TrySetVersionNumber();
        }

        private void TrySetVersionNumber()
		{
            try
            {
				var version = Assembly.GetExecutingAssembly().GetName()?.Version?.ToString();
				this.Dispatcher.Invoke(() => {
					versionNum.Text = $"v{version}";
				});
            }
            catch
            {
                this.Dispatcher.Invoke(() => {
                    versionNum.Text = "";
                });
            }
        }

		private void Close(object sender, RoutedEventArgs e)
        {
			this.Close();
        }

        private void Minimize(object sender, RoutedEventArgs e)
        {
			SystemCommands.MinimizeWindow(this);
		}

        private void Maximize(object sender, RoutedEventArgs e)
        {
			if (this.WindowState != WindowState.Maximized)
				SystemCommands.MaximizeWindow(this);
			else
                SystemCommands.RestoreWindow(this);
        }
    }
}
