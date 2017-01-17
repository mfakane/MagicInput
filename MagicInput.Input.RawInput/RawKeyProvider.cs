using System.Collections.Generic;
using System.Linq;
using Linearstar.Core.RawInput;

namespace MagicInput.Input.RawInput
{
	public class RawKeyProvider : IKeyProvider
	{
		public IEnumerable<KeyPhysicalDevice> GetAvailablePhysicalDevices() =>
			RawInputDevice.GetDevices().Where(i => i.IsConnected && RawKeyPhysicalDevice.IsSupported(i)).Select(i => new RawKeyPhysicalDevice(i)).ToArray();
	}
}
