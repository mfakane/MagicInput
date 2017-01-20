using Linearstar.Core.RawInput;

namespace MagicInput.Input.RawInput
{
	static class RawMouseButtonFlagsEx
	{
		public static MouseButtons ToMouseButtons(this RawMouseButtonFlags flags)
		{
			var rt = MouseButtons.None;

			if ((flags & (RawMouseButtonFlags.LeftButtonDown)) != 0)
				rt |= MouseButtons.Left;

			if ((flags & (RawMouseButtonFlags.RightButtonDown)) != 0)
				rt |= MouseButtons.Right;

			if ((flags & (RawMouseButtonFlags.MiddleButtonDown)) != 0)
				rt |= MouseButtons.Middle;

			if ((flags & (RawMouseButtonFlags.Button4Down)) != 0)
				rt |= MouseButtons.X1;

			if ((flags & (RawMouseButtonFlags.Button5Down)) != 0)
				rt |= MouseButtons.X2;

			if ((flags & RawMouseButtonFlags.MouseWheel) != 0)
				rt |= MouseButtons.Wheel;

			if ((flags & RawMouseButtonFlags.MouseHorizontalWheel) != 0)
				rt |= MouseButtons.HorizontalWheel;

			return rt;
		}

		public static RawKeyMouseButtons ToRawKeyMouseButton(this RawMouseButtonFlags flags, int wheelDelta)
		{
			var rt = RawKeyMouseButtons.None;

			if ((flags & (RawMouseButtonFlags.LeftButtonDown | RawMouseButtonFlags.LeftButtonUp)) != 0)
				rt |= RawKeyMouseButtons.Left;

			if ((flags & (RawMouseButtonFlags.RightButtonDown | RawMouseButtonFlags.RightButtonUp)) != 0)
				rt |= RawKeyMouseButtons.Right;

			if ((flags & (RawMouseButtonFlags.MiddleButtonDown | RawMouseButtonFlags.MiddleButtonUp)) != 0)
				rt |= RawKeyMouseButtons.Middle;

			if ((flags & (RawMouseButtonFlags.Button4Down | RawMouseButtonFlags.Button4Up)) != 0)
				rt |= RawKeyMouseButtons.X1;

			if ((flags & (RawMouseButtonFlags.Button5Down | RawMouseButtonFlags.Button5Up)) != 0)
				rt |= RawKeyMouseButtons.X2;

			if ((flags & RawMouseButtonFlags.MouseWheel) != 0)
				rt |= wheelDelta > 0 ? RawKeyMouseButtons.WheelUp : RawKeyMouseButtons.WheelDown;

			if ((flags & RawMouseButtonFlags.MouseHorizontalWheel) != 0)
				rt |= wheelDelta > 0 ? RawKeyMouseButtons.HorizontalWheelLeft : RawKeyMouseButtons.HorizontalWheelRight;

			return rt;
		}

		public static RawKeyMouseButtons GetDownRawKeyMouseButton(this RawMouseButtonFlags flags, int wheelDelta, bool firstOnly = false)
		{
			var rt = RawKeyMouseButtons.None;

			if ((flags & RawMouseButtonFlags.LeftButtonDown) != 0)
			{
				rt |= RawKeyMouseButtons.Left;

				if (firstOnly) return rt;
			}

			if ((flags & RawMouseButtonFlags.RightButtonDown) != 0)
			{
				rt |= RawKeyMouseButtons.Right;

				if (firstOnly) return rt;
			}

			if ((flags & RawMouseButtonFlags.MiddleButtonDown) != 0)
			{
				rt |= RawKeyMouseButtons.Middle;

				if (firstOnly) return rt;
			}

			if ((flags & RawMouseButtonFlags.Button4Down) != 0)
			{
				rt |= RawKeyMouseButtons.X1;

				if (firstOnly) return rt;
			}

			if ((flags & RawMouseButtonFlags.Button5Down) != 0)
			{
				rt |= RawKeyMouseButtons.X2;

				if (firstOnly) return rt;
			}

			if ((flags & RawMouseButtonFlags.MouseWheel) != 0)
			{
				rt |= wheelDelta > 0 ? RawKeyMouseButtons.WheelUp : RawKeyMouseButtons.WheelDown;

				if (firstOnly) return rt;
			}

			if ((flags & RawMouseButtonFlags.MouseHorizontalWheel) != 0)
			{
				rt |= wheelDelta > 0 ? RawKeyMouseButtons.HorizontalWheelLeft : RawKeyMouseButtons.HorizontalWheelRight;

				if (firstOnly) return rt;
			}

			return rt;
		}
	}
}
