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

namespace StockSimulator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public SimulatorConfig Config { get; set; }
		public Simulator Sim { get; set; }

		public MainWindow()
		{
			InitializeComponent();

			Config = new SimulatorConfig();
			_propertyGrid.SelectedObject = Config;
		}

		private void _runButton_Click(object sender, RoutedEventArgs e)
		{
			Sim = new Simulator();

			// Create the simulator and then add an item to display it status.
			Sim.CreateFromConfig(Config);
			// For each instrument
			// create list item

			// Initializes all the instruments. Their status can then be updated in the gui.
			Sim.Initialize();
		}
	}
}
