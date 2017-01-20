using System;
using System.Linq;
using System.Text;
using Linearstar.Core.RawInput;

namespace MagicInput.Input.RawInput
{
	public class RawKeyInput : KeyInput, IEquatable<RawKeyInput>
	{
		public override string DisplayCode =>
			((int?)MouseButton)?.ToString() ?? ScanCode?.ToString("X2");
		public override string ShortName =>
			MouseButton.HasValue
				? new string(MouseButton.ToString().Where(i => char.IsUpper(i) || char.IsDigit(i)).ToArray()).TrimStart('H')
				: ToString();
		public RawKeyMouseButtons? MouseButton { get; set; }
		public int? ScanCode { get; set; }

		public RawKeyInput()
		{
		}

		public RawKeyInput(RawKeyMouseButtons mouseButton) =>
			MouseButton = mouseButton;

		public RawKeyInput(int scanCode) =>
			ScanCode = scanCode;

		public bool IsMatch(RawInputData rid, bool isUp)
		{
			if (MouseButton.HasValue && rid.Type == RawInputDeviceType.Mouse)
				return !isUp && (rid.Mouse.Buttons.GetDownRawKeyMouseButton(rid.Mouse.ButtonData) & MouseButton) != 0
					|| isUp && (rid.Mouse.Buttons.ToRawKeyMouseButton(rid.Mouse.ButtonData) & MouseButton) != 0 && (rid.Mouse.Buttons.GetDownRawKeyMouseButton(rid.Mouse.ButtonData) & MouseButton) == 0;

			if (ScanCode.HasValue && rid.Type == RawInputDeviceType.Keyboard && rid.Keyboard.ScanCode == ScanCode)
				return ((rid.Keyboard.Flags & RawKeyboardFlags.Up) != 0) == isUp;

			return false;
		}

		public override string ToString()
		{
			if (ScanCode.HasValue)
			{
				var sb = new StringBuilder(16);

				NativeMethods.GetKeyNameText((uint)ScanCode << 16, sb, sb.Capacity);

				return sb.ToString();
			}
			else if (MouseButton.HasValue)
				return MouseButton.ToString();
			else
				return null;
		}

		public bool Equals(RawKeyInput other) =>
			other != null &&
			(other.DeviceGuid == Guid.Empty || DeviceGuid == Guid.Empty || other.DeviceGuid == DeviceGuid) &&
			(other.ScanCode.HasValue && ScanCode.HasValue && other.ScanCode == ScanCode ||
			other.MouseButton.HasValue && MouseButton.HasValue && other.MouseButton == MouseButton);

		public override bool Equals(object obj) =>
			obj is RawKeyInput rki && Equals(rki);

		public override int GetHashCode() =>
			typeof(RawKeyInput).GetHashCode() ^ ScanCode.GetHashCode();

		public static RawKeyInput FromRawInputData(RawInputData rid)
		{
			switch (rid.Type)
			{
				case RawInputDeviceType.Mouse:
					{
						var button = rid.Mouse.Buttons.GetDownRawKeyMouseButton(rid.Mouse.ButtonData, true);

						return button != RawKeyMouseButtons.None ? new RawKeyInput
						{
							MouseButton = button,
						} : null;
					}
				case RawInputDeviceType.Keyboard:
					return new RawKeyInput
					{
						ScanCode = rid.Keyboard.ScanCode,
					};
				default:
					throw new NotSupportedException();
			}
		}
	}

	[Flags]
	public enum RawKeyMouseButtons
	{
		None,
		Left,
		Right,
		Middle = 4,
		X1 = 8,
		X2 = 16,
		WheelUp = 32,
		WheelDown = 64,
		HorizontalWheelLeft = 128,
		HorizontalWheelRight = 256,
	}
}
