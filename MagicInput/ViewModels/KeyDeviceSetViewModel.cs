using System.Collections.Generic;
using Livet;
using Livet.Messaging;
using MagicInput.Input;

namespace MagicInput.ViewModels
{
	class KeyDeviceSetViewModel : ViewModelBase
	{
		public MainViewModel Main { get; }
		public KeyDeviceSet DeviceSet { get; }
		public IReadOnlyList<KeyDeviceViewModel> Devices { get; }
		public IReadOnlyList<KeyProfileViewModel> Profiles { get; }

		public KeyDeviceSetViewModel(MainViewModel main, KeyDeviceSet deviceSet)
		{
			Main = main;
			DeviceSet = deviceSet;
			Devices = RegisterDisposable(ViewModelHelper.CreateReadOnlyDispatcherCollection(deviceSet.Devices, i => new KeyDeviceViewModel(this, i), DispatcherHelper.UIDispatcher));
			Profiles = RegisterDisposable(ViewModelHelper.CreateReadOnlyDispatcherCollection(deviceSet.Profiles, i => new KeyProfileViewModel(this, i), DispatcherHelper.UIDispatcher));
		}

		public void ShowSettings() =>
			Main.Messenger.Raise(new TransitionMessage(new KeyDeviceSetSettingsViewModel(Main, DeviceSet), nameof(KeyDeviceSetSettingsViewModel)));
	}
}
