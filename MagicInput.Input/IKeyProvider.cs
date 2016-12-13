using System.Collections.Generic;

namespace MagicInput.Input
{
	public interface IKeyProvider
    {
		IEnumerable<KeyPhysicalDevice> GetAvailablePhysicalDevices();
	}
}
