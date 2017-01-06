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

		void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			var treeView = (TreeView)sender;

			if (e.NewValue is KeyDeviceSetViewModel deviceSet)
				ViewModel.SelectedProfile = deviceSet.Profiles.FirstOrDefault();
			else if (e.NewValue is KeyProfileViewModel profile)
				ViewModel.SelectedProfile = profile;
		}
	}
}
