using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Linearstar.Core.RawInput;
using MagicInput.Input.Behaviors;
using MsgPack.Serialization;

namespace MagicInput.Input.RawInput
{
	public class RawKeyInput : KeyInput, IEquatable<RawKeyInput>
	{
		public override string DisplayCode =>
			((int?)MouseButton)?.ToString() ?? ScanCode?.ToString("X2");
		public override string ShortName =>
			MouseButton.HasValue
				? MouseButton == RawKeyMouseButtons.Move ? "P" : new string(MouseButton.ToString().Where(i => char.IsUpper(i) || char.IsDigit(i)).ToArray()).TrimStart('H')
				: ToString();
		public RawKeyMouseButtons? MouseButton { get; set; }
		public int? ScanCode { get; set; }

		internal DateTime LastInput { get; set; }

		public RawKeyInput()
		{
		}

		public RawKeyInput(RawKeyMouseButtons mouseButton) =>
			MouseButton = mouseButton;

		public RawKeyInput(int scanCode) =>
			ScanCode = scanCode;

		[MessagePackIgnore]
		public override IList<Type> SupportedTypes =>
			MouseButton == RawKeyMouseButtons.Move ? new[]
			{
				typeof(FallbackBehavior),
				typeof(MoveWithSequenceBehavior),
			} : base.SupportedTypes;

		public bool IsMatch(RawInputData rid, bool isUp)
		{
			if (MouseButton.HasValue && rid.Type == RawInputDeviceType.Mouse)
				if (isUp)
					return (rid.Mouse.Buttons.ToRawKeyMouseButton(rid.Mouse.ButtonData) & MouseButton) != 0 && (rid.Mouse.Buttons.GetDownRawKeyMouseButton(rid.Mouse.ButtonData) & MouseButton) == 0;
				else
					return (rid.Mouse.Buttons.GetDownRawKeyMouseButton(rid.Mouse.ButtonData) & MouseButton) != 0
						|| (rid.Mouse.LastX != 0 || rid.Mouse.LastY != 0) && (MouseButton & RawKeyMouseButtons.Move) != 0;

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
		Move,
		Left,
		Right = 4,
		Middle = 8,
		X1 = 16,
		X2 = 32,
		WheelUp = 64,
		WheelDown = 128,
		HorizontalWheelLeft = 256,
		HorizontalWheelRight = 512,
	}
}
