using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using Livet;
using Livet.EventListeners;
using Livet.Messaging;
using MagicInput.Input;

namespace MagicInput.ViewModels
{
	public class KeyDeviceSetViewModel : ViewModelBase
	{
		public MainViewModel Main { get; }
		public KeyDeviceSet DeviceSet { get; }
		public IReadOnlyList<KeyDeviceViewModel> Devices { get; }
		public IReadOnlyList<KeyProfileViewModel> Profiles { get; }

		public KeyProfileViewModel SelectedProfile
		{
			get => Profiles.FirstOrDefault(i => i.Profile == DeviceSet.CurrentProfile);
			set => DeviceSet.CurrentProfile = value.Profile;
		}

		public KeyDeviceSetViewModel(MainViewModel main, KeyDeviceSet deviceSet)
		{
			Main = main;
			DeviceSet = deviceSet;
			Devices = RegisterDisposable(ViewModelHelper.CreateReadOnlyDispatcherCollection(deviceSet.Devices, i => new KeyDeviceViewModel(this, i), DispatcherHelper.UIDispatcher));
			Profiles = RegisterDisposable(ViewModelHelper.CreateReadOnlyDispatcherCollection(deviceSet.Profiles, i => new KeyProfileViewModel(this, i), DispatcherHelper.UIDispatcher));

			CompositeDisposable.Add(new PropertyChangedEventListener(DeviceSet)
			{
				{ nameof(DeviceSet.CurrentProfile), (sender, e) => DispatcherHelper.UIDispatcher.InvokeAsync(() => RaisePropertyChanged(nameof(SelectedProfile))) },
			});
		}

		public void AddProfile()
		{
			DeviceSet.Profiles.Add(new KeyProfile("プロファイル " + Profiles.Count)
			{
				KeyMaps =
				{
					new KeyMap("デフォルト"),
				},
			});
			CollectionViewSource.GetDefaultView(Profiles).MoveCurrentTo(Profiles.Last());
			Main.Model.ApplyChanges();
		}

		public void RemoveProfile(KeyProfileViewModel profile)
		{
			var cvs = CollectionViewSource.GetDefaultView(Profiles);

			if (cvs.CurrentPosition >= Profiles.Count - 1)
				cvs.MoveCurrentToPrevious();

			DeviceSet.Profiles.Remove(profile.Profile);
			Main.Model.ApplyChanges();
		}

		public void ShowSettings() =>
			Main.Messenger.Raise(new TransitionMessage(new KeyDeviceSetSettingsViewModel(Main, DeviceSet), nameof(KeyDeviceSetSettingsViewModel)));
	}
}
