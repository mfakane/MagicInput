using System.Threading;
using System.Windows;

namespace MagicInput.Input.Behaviors.Actions
{
	public class CopyToClipboardAction : SequenceAction
	{
		public string Text { get; set; }

		public override void DoAction()
		{
			if (!string.IsNullOrEmpty(Text))
			{
				var t = new Thread(() => Clipboard.SetText(Text));

				t.SetApartmentState(ApartmentState.STA);
				t.Start();
				t.Join();
			}
		}

		public override string ToString()
		{
			return "コピー " + Ellipsis(Text?.Replace("\r", null).Replace("\n", null), 16);

			string Ellipsis(string str, int length) =>
				length < (str?.Length ?? 0) ? str.Substring(0, length) + "..." : str;
		}
	}
}
