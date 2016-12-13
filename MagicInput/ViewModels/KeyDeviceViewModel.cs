using MagicInput.Input;

namespace MagicInput.ViewModels
{
	public class KeyDeviceViewModel : ViewModelBase
	{
		public KeyDeviceSetViewModel DeviceSet { get; }
		public KeyDevice Device { get; }

		public KeyDeviceViewModel(KeyDeviceSetViewModel deviceSet, KeyDevice device)
		{
			DeviceSet = deviceSet;
			Device = device;
		}
	}
}
