using System;

namespace MagicInput.Input
{
	public class KeyInputEventArgs : EventArgs
	{
		public KeyPhysicalDevice PhysicalDevice { get; }
		public KeyInput Key { get; }
		public bool Handled { get; set; }

		public KeyInputEventArgs(KeyPhysicalDevice physicalDevice, KeyInput key)
		{
			PhysicalDevice = physicalDevice;
			Key = key;
		}
	}
}
