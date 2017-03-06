using System;
using System.Text;
using Linearstar.Core.RawInput;

namespace MagicInput.Input.RawInput
{
	public class RawKeyInput : RawKeyInputBase, IEquatable<RawKeyInput>
	{
		public override string DisplayCode => ScanCode.ToString("X2");
		public override string ShortName => ToString();

		public int ScanCode { get; set; }

		public RawKeyInput()
		{
		}

		public RawKeyInput(int scanCode) =>
			ScanCode = scanCode;

		public static RawKeyInput FromRawInputData(RawInputData rid)
		{
			if (rid.Type != RawInputDeviceType.Keyboard) return null;

			return new RawKeyInput
			{
				ScanCode = rid.Keyboard.ScanCode,
			};
		}

		public override bool IsMatch(RawInputData rid, bool isUp)
		{
			if (rid.Type != RawInputDeviceType.Keyboard ||
				rid.Keyboard.ScanCode != ScanCode)
				return false;

			return ((rid.Keyboard.Flags & RawKeyboardFlags.Up) != 0) == isUp;
		}

		public override string ToString()
		{
			var sb = new StringBuilder(16);

			NativeMethods.GetKeyNameText((uint)ScanCode << 16, sb, sb.Capacity);

			return sb.ToString();
		}

		public bool Equals(RawKeyInput other) =>
			(other?.DeviceGuid == Guid.Empty || DeviceGuid == Guid.Empty || other?.DeviceGuid == DeviceGuid) &&
			other?.ScanCode == ScanCode;

		public override bool Equals(object obj) =>
			obj is RawKeyInput rki && Equals(rki);

		public override int GetHashCode() =>
			typeof(RawKeyInput).GetHashCode() ^ ScanCode.GetHashCode();
	}
}
