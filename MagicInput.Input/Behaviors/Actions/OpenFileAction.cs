using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace MagicInput.Input.Behaviors.Actions
{
	public class OpenFileAction : SequenceAction
	{
		string _fileName;
		public string FileName
		{
			get => _fileName;
			set
			{
				if (_fileName != (_fileName = value))
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(FileName)));
			}
		}

		public string Arguments { get; set; }

		public override void DoAction() =>
			Process.Start(new ProcessStartInfo(FileName, Arguments));

		public override string ToString()
		{
			return $"開く {Path.GetFileName(FileName)} {Arguments}";
		}
	}
}
