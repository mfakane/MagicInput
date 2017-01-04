using System.ComponentModel;
using System.Linq;
using Linearstar.Core.SendInput;

namespace MagicInput.Input.Behaviors.Actions
{
	public class KeyStrokeAction : SequenceAction
	{
		VirtualKey[] virtualKeys;

		public VirtualKey[] VirtualKeys
		{
			get => virtualKeys;
			set
			{
				virtualKeys = value;
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(VirtualKeys)));
			}
		}

		public KeyStrokeActionKind Kind { get; set; }

		public override void DoAction()
		{
			if (Kind == KeyStrokeActionKind.DownUp)
				SendInput.Send(VirtualKeys.Select(i => (SendInputData)new SendKeyboard((short)i, GetFlags(i)))
										  .Concat(VirtualKeys.Reverse().Select(i => (SendInputData)new SendKeyboard((short)i, GetFlags(i) | KeyboardEventFlags.KeyUp))));
			else
				SendInput.Send(VirtualKeys.Select(i =>
					(SendInputData)new SendKeyboard((short)i, GetFlags(i) | (Kind == KeyStrokeActionKind.Up ? KeyboardEventFlags.KeyUp : KeyboardEventFlags.None))));
		}

		static KeyboardEventFlags GetFlags(VirtualKey vk) =>
			vk.IsExtendedKey() ? KeyboardEventFlags.ExtendedKey : KeyboardEventFlags.None;

		public override string ToString() =>
			$"キー {string.Join("+", VirtualKeys)} {Kind}";
	}

	public enum KeyStrokeActionKind
	{
		Down,
		Up,
		DownUp,
	}
}
