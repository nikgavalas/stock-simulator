using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

using StockSimulator.Core;
using System.Threading;
using System.IO;
using Newtonsoft.Json;

namespace StockSimulator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// Holds all the raw price data for the sim.
		/// </summary>
		private TickerDataStore DataStore { get; set; }

		/// <summary>
		/// Config data that comes from the property grid.
		/// </summary>
		private SimulatorConfig Config { get; set; }

		/// <summary>
		/// Our main sim. Gets created each run.
		/// </summary>
		public Simulator Sim { get; set; }

		private CancellationTokenSource _cancelToken;

		public MainWindow()
		{
			InitializeComponent();

			_cancelToken = new CancellationTokenSource();
			Config = new SimulatorConfig();
			DataStore = new TickerDataStore();
			_propertyGrid.SelectedObject = Config;

			InitFromCommandLine();
		}

		private async void _runButton_Click(object sender, RoutedEventArgs e)
		{
			// Disable the button while running.
			EnableOptions(false);

			_statusText.Text = "";

			Progress<string> progress = new Progress<string>(data => UpdateStatus(data));
			try
			{
				await Task.Run(() => RunSim(progress, _cancelToken.Token));
			}
			catch (OperationCanceledException)
			{
				// TODO: Update the gui to indicator the method was canceled.
			}

			EnableOptions(true);
		}

		private void RunSim(IProgress<string> progress, CancellationToken cancelToken)
		{
			Sim = new Simulator(progress, cancelToken);

			// Create the simulator.
			Sim.CreateFromConfig(Config, DataStore);

			// Initializes all the instruments.
			Sim.Initialize();

			// Runs the simulation.
			Sim.Run();

			// Output all the data.
			Sim.Shutdown();
		}

		private void InitFromCommandLine()
		{
			string configOption = "-config:";

			string[] args = Environment.GetCommandLineArgs();
			if (args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i].StartsWith(configOption))
					{
						_configFilePath.Text = args[i].Substring(configOption.Length, args[i].Length - configOption.Length);
						try
						{
							InitConfigFromFile();
						}
						catch (Exception e)
						{
							UpdateStatus("Could not init config from path on the command line: " + e.Message);
						}
					}
				}
			}
		}

		private void UpdateStatus(string message)
		{
			_statusText.Focus();
			_statusText.Text += DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff-") + message + Environment.NewLine;
			_statusText.CaretIndex = _statusText.Text.Length;
			_statusText.ScrollToEnd();
		}

		private void EnableOptions(bool shouldEnable)
		{
			_runButton.IsEnabled = shouldEnable;
			_dataMenu.IsEnabled = shouldEnable;
		}

		private void InitConfigFromFile()
		{
			string json = File.ReadAllText(_configFilePath.Text);
			Config = JsonConvert.DeserializeObject<SimulatorConfig>(json);
			_propertyGrid.SelectedObject = Config;
			UpdateStatus("Config file loaded from " + _configFilePath.Text);			
		}

		private void _clearCache_Click(object sender, RoutedEventArgs e)
		{
			EnableOptions(false);
			UpdateStatus("Start cleaning ticker cache");

			Task t = Task.Run(() =>
			{
				DataStore.ClearCache();
			});

			t.Wait();
			EnableOptions(true);
			UpdateStatus("Finished cleaning ticker cache");
		}

		private void _clearOutput_Click(object sender, RoutedEventArgs e)
		{
			EnableOptions(false);
			UpdateStatus("Start cleaning output folder");

			Task t = Task.Run(() =>
			{
				DirectoryInfo cacheInfo = new DirectoryInfo(Config.OutputFolder);
				foreach (DirectoryInfo item in cacheInfo.GetDirectories())
				{
					if (item.Name != "test")
					{
						item.Delete(true);
					}
				}
			});

			t.Wait();
			EnableOptions(true);
			UpdateStatus("Finished cleaning output folder");
		}

		private void _loadButton_Click(object sender, RoutedEventArgs e)
		{
			// Configure open file dialog box
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.DefaultExt = ".json"; // Default file extension
			dlg.Filter = "Config File (.json)|*.json"; // Filter files by extension 

			// Show open file dialog box
			Nullable<bool> result = dlg.ShowDialog();

			// Process open file dialog box results 
			if (result == true)
			{
				// Open document 
				_configFilePath.Text = dlg.FileName;
				InitConfigFromFile();
			}
		}

		private void _saveButton_Click(object sender, RoutedEventArgs e)
		{
			// Configure save file dialog box
			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
			dlg.FileName = "Config"; // Default file name
			dlg.DefaultExt = ".json"; // Default file extension
			dlg.Filter = "Config File (.json)|*.json"; // Filter files by extension 

			// Show save file dialog box
			Nullable<bool> result = dlg.ShowDialog();

			// Process save file dialog box results 
			if (result == true)
			{
				// Save document 
				string filename = dlg.FileName;
				string json = JsonConvert.SerializeObject(Config, Formatting.Indented);
				File.WriteAllText(filename, json);
			}
		}
	}
}
