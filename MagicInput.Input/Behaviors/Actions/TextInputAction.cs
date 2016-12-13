using System.Windows.Forms;

namespace MagicInput.Input.Behaviors.Actions
{
	public class TextInputAction : SequenceAction
	{
		public string Text { get; set; }

		public override void DoAction()
		{
			if (!string.IsNullOrEmpty(Text))
				SendKeys.SendWait(Text);
		}

		public override string ToString()
		{
			return "テキスト " + Ellipsis(Text?.Replace("\r", null).Replace("\n", null), 16);

			string Ellipsis(string str, int length) =>
				length < (str?.Length ?? 0) ? str.Substring(0, length) + "..." : str;
		}
	}
}
