using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MagicInput.ViewModels;

namespace MagicInput.Views
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		MainViewModel ViewModel => (MainViewModel)DataContext;

		public MainWindow() =>
			InitializeComponent();

		void Window_SourceInitialized(object sender, EventArgs e) =>
			ViewModel.Model.Load(this);
	}
}
