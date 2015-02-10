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
		public SimulatorConfig Config { get; set; }
		public Simulator Sim { get; set; }

		private CancellationTokenSource _cancelToken;

		public MainWindow()
		{
			InitializeComponent();

			_cancelToken = new CancellationTokenSource();
			Config = new SimulatorConfig();
			_propertyGrid.SelectedObject = Config;
		}

		private async void _runButton_Click(object sender, RoutedEventArgs e)
		{
			// Disable the button while running.
			_runButton.IsEnabled = false;

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
			Sim.CreateFromConfig(Config);

			// Initializes all the instruments.
			Sim.Initialize();

			// Runs the simulation.
			Sim.Run();

			// Output all the data.
			Sim.Shutdown();
		}

		private void UpdateStatus(string message)
		{
			_statusLabel.Content = message;
		}
	}
}
