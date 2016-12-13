namespace MagicInput.Input
{
	public static class VirtualKeyEx
	{
		public static bool IsExtendedKey(this VirtualKey virtualKey)
		{
			switch (virtualKey)
			{
				case VirtualKey.NumLock:
				case VirtualKey.Insert:
				case VirtualKey.Delete:
				case VirtualKey.Home:
				case VirtualKey.End:
				case VirtualKey.PageUp:
				case VirtualKey.PageDown:
				case VirtualKey.Up:
				case VirtualKey.Down:
				case VirtualKey.Left:
				case VirtualKey.Right:
				case VirtualKey.App:
				case VirtualKey.LeftWin:
				case VirtualKey.RightWin:
				case VirtualKey.NumPadDiv:
				case VirtualKey.NumPadEnter:
					return true;
				default:
					return false;
			}
		}
	}
}
