using System;

namespace MagicInput.Input
{
	[Flags]
	public enum MouseButtons : byte
	{
		None,
		Left,
		Right,
		Middle = 4,
		X1 = 8,
		X2 = 16,
		Wheel = 32,
		HorizontalWheel = 64,
	}
}
