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
		}

		private async void _runButton_Click(object sender, RoutedEventArgs e)
		{
			// Disable the button while running.
			_runButton.IsEnabled = false;

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

			_runButton.IsEnabled = true;
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

		private void UpdateStatus(string message)
		{
			_statusText.Focus();
			_statusText.Text += DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff-") + message + Environment.NewLine;
			_statusText.CaretIndex = _statusText.Text.Length;
			_statusText.ScrollToEnd();
		}

		private void _clearCache_Click(object sender, RoutedEventArgs e)
		{
			DataStore.ClearCache();
		}
	}
}
