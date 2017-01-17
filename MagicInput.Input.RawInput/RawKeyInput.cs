using System;
using System.Collections.Generic;
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
		public RawKeyMouseButton? MouseButton { get; set; }
		public int? ScanCode { get; set; }

		public RawKeyInput()
		{
		}

		public RawKeyInput(RawKeyMouseButton mouseButton) =>
			MouseButton = mouseButton;

		public RawKeyInput(int scanCode) =>
			ScanCode = scanCode;

		public bool IsMatch(RawInputData rid, bool isUp)
		{
			if (MouseButton.HasValue && rid.Type == RawInputDeviceType.Mouse)
			{
				var buttons = rid.Mouse.Buttons;

				if (isUp)
					return MouseButton == RawKeyMouseButton.Left && (buttons & RawMouseButtonFlags.LeftButtonUp) != 0
						|| MouseButton == RawKeyMouseButton.Right && (buttons & RawMouseButtonFlags.RightButtonUp) != 0
						|| MouseButton == RawKeyMouseButton.Middle && (buttons & RawMouseButtonFlags.MiddleButtonUp) != 0
						|| MouseButton == RawKeyMouseButton.X1 && (buttons & RawMouseButtonFlags.Button4Up) != 0
						|| MouseButton == RawKeyMouseButton.X2 && (buttons & RawMouseButtonFlags.Button5Up) != 0;
				else
					return GetMouseButtons(rid).Contains(MouseButton.Value);
			}

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
						var button = GetMouseButtons(rid).ToArray();

						return button.Any() ? new RawKeyInput
						{
							MouseButton = button.First(),
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

		public static IEnumerable<RawKeyMouseButton> GetMouseButtons(RawInputData rid)
		{
			if ((rid.Mouse.Buttons & RawMouseButtonFlags.LeftButtonDown) != 0 ||
				(rid.Mouse.Buttons & RawMouseButtonFlags.LeftButtonUp) != 0)
				yield return RawKeyMouseButton.Left;

			if ((rid.Mouse.Buttons & RawMouseButtonFlags.RightButtonUp) != 0 ||
				(rid.Mouse.Buttons & RawMouseButtonFlags.RightButtonUp) != 0)
				yield return RawKeyMouseButton.Right;

			if ((rid.Mouse.Buttons & RawMouseButtonFlags.MiddleButtonDown) != 0 ||
				(rid.Mouse.Buttons & RawMouseButtonFlags.MiddleButtonUp) != 0)
				yield return RawKeyMouseButton.Middle;

			if ((rid.Mouse.Buttons & RawMouseButtonFlags.Button4Down) != 0 ||
				(rid.Mouse.Buttons & RawMouseButtonFlags.Button4Up) != 0)
				yield return RawKeyMouseButton.X1;

			if ((rid.Mouse.Buttons & RawMouseButtonFlags.Button5Down) != 0 ||
				(rid.Mouse.Buttons & RawMouseButtonFlags.Button5Up) != 0)
				yield return RawKeyMouseButton.X2;

			if ((rid.Mouse.Buttons & RawMouseButtonFlags.MouseWheel) != 0)
				yield return rid.Mouse.ButtonData > 0 ? RawKeyMouseButton.WheelUp : RawKeyMouseButton.WheelDown;

			if ((rid.Mouse.Buttons & RawMouseButtonFlags.MouseHorizontalWheel) != 0)
				yield return rid.Mouse.ButtonData > 0 ? RawKeyMouseButton.HorizontalWheelLeft : RawKeyMouseButton.HorizontalWheelRight;
		}
	}

	public enum RawKeyMouseButton
	{
		None,
		Left,
		Right,
		Middle,
		X1,
		X2,
		WheelUp,
		WheelDown,
		HorizontalWheelLeft,
		HorizontalWheelRight,
	}
}
