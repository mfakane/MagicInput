using Linearstar.Core.SendInput;

namespace MagicInput.Input.Behaviors.Actions
{
	public class KeyStrokeAction : SequenceAction
	{
		public VirtualKey VirtualKey { get; set; }
		public KeyStrokeActionKind Kind { get; set; }

		public override void DoAction()
		{
			if (Kind == KeyStrokeActionKind.DownUp)
				SendInput.Send(new SendKeyboard((short)VirtualKey, KeyboardEventFlags.None), new SendKeyboard((short)VirtualKey, KeyboardEventFlags.KeyUp));
			else
				SendInput.Send(new SendKeyboard((short)VirtualKey, Kind == KeyStrokeActionKind.Up ? KeyboardEventFlags.KeyUp : KeyboardEventFlags.None));
		}

		public override string ToString() =>
			$"キー {VirtualKey} {Kind}";
	}

	public enum KeyStrokeActionKind
	{
		Down,
		Up,
		DownUp,
	}
}
