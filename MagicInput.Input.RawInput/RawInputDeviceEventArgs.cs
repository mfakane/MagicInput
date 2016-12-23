using System;
using Linearstar.Core.RawInput;

namespace MagicInput.Input.RawInput
{
	class RawInputDeviceEventArgs : EventArgs
	{
		public RawInputDevice Device { get; }
		public IntPtr Handle { get; }

		public RawInputDeviceEventArgs(IntPtr handle) =>
			Handle = handle;

		public RawInputDeviceEventArgs(RawInputDevice device)
			: this(device.Handle) =>
			Device = device;
	}
}
