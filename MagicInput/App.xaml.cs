using System.Windows;
using System.Windows.Interop;
using Livet;
using MagicInput.Views;

namespace MagicInput
{
	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Application
	{
		void Application_Startup(object sender, StartupEventArgs e)
		{
			DispatcherHelper.UIDispatcher = Dispatcher;

			var window = new MainWindow();
			var helper = new WindowInteropHelper(window);

			helper.EnsureHandle();
		}
	}
}
