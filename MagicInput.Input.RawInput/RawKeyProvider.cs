using System.Collections.Generic;
using System.Linq;
using Linearstar.Core.RawInput;

namespace MagicInput.Input.RawInput
{
	public class RawKeyProvider : IKeyProvider
	{
		public IEnumerable<KeyPhysicalDevice> GetAvailablePhysicalDevices() =>
			RawInputDevice.GetDevices().Where(i => i.IsConnected && (i.DeviceType == RawInputDeviceType.Mouse || i.DeviceType == RawInputDeviceType.Keyboard)).Select(i => new RawKeyPhysicalDevice(i)).ToArray();
	}
}
