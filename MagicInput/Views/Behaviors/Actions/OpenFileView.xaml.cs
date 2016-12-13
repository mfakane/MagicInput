using System.Windows.Controls;
using Livet.Messaging.IO;
using MagicInput.Input.Behaviors.Actions;

namespace MagicInput.Views.Actions
{
	/// <summary>
	/// OpenFileView.xaml の相互作用ロジック
	/// </summary>
	public partial class OpenFileView : UserControl
	{
		public OpenFileView()
		{
			InitializeComponent();
		}

		public void SelectFile(OpeningFileSelectionMessage message)
		{
			if (message.Response != null)
				((OpenFileAction)DataContext).FileName = message.Response[0];
		}
	}
}
