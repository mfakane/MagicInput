using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using Livet;
using Livet.EventListeners;
using Livet.Messaging;
using MagicInput.Input;

namespace MagicInput.ViewModels
{
	class KeyProfileViewModel : ViewModelBase
	{
		public KeyMapViewModel CurrentKeyMap
		{
			get => KeyMaps[Profile.KeyMaps.IndexOf(Profile.CurrentKeyMap)];
			set => Profile.CurrentKeyMap = Profile.KeyMaps[Profile.KeyMaps.IndexOf(value.KeyMap)];
		}

		public bool IsSelected
		{
			get => Profile.DeviceSet.CurrentProfile == Profile;
			set
			{
				if (value)
					Profile.DeviceSet.CurrentProfile = Profile;
			}
		}

		public KeyDeviceSetViewModel DeviceSet { get; }
		public KeyProfile Profile { get; }
		public IReadOnlyList<KeyMapViewModel> KeyMaps { get; }

		public KeyProfileViewModel(KeyDeviceSetViewModel deviceSet, KeyProfile profile)
		{
			DeviceSet = deviceSet;
			Profile = profile;
			KeyMaps = RegisterDisposable(ViewModelHelper.CreateReadOnlyDispatcherCollection(profile.KeyMaps, i => new KeyMapViewModel(this, i), DispatcherHelper.UIDispatcher));

			CompositeDisposable.Add(new PropertyChangedEventListener(Profile)
			{
				{ nameof(CurrentKeyMap), (sender, e) => DispatcherHelper.UIDispatcher.InvokeAsync(() => RaisePropertyChanged(nameof(CurrentKeyMap))) },
			});
			CompositeDisposable.Add(new PropertyChangedEventListener(Profile.DeviceSet)
			{
				{ nameof(Profile.DeviceSet.CurrentProfile), (sender, e) => DispatcherHelper.UIDispatcher.InvokeAsync(() => RaisePropertyChanged(nameof(IsSelected))) },
			});
			((IEditableCollectionView)CollectionViewSource.GetDefaultView(KeyMaps)).NewItemPlaceholderPosition = NewItemPlaceholderPosition.AtEnd;
		}

		public void AddKeyMap()
		{
			Profile.KeyMaps.Add(new KeyMap("キーマップ " + Profile.KeyMaps.Count, DeviceSet.DeviceSet.GetAllKeys()));
			CollectionViewSource.GetDefaultView(KeyMaps).MoveCurrentToLast();
			DeviceSet.Main.Model.ApplyChanges();
		}

		public void RemoveKeyMap(KeyMapViewModel keyMap)
		{
			var cvs = CollectionViewSource.GetDefaultView(KeyMaps);

			if (cvs.CurrentPosition >= KeyMaps.Count - 1)
				cvs.MoveCurrentToPrevious();

			Profile.KeyMaps.Remove(keyMap.KeyMap);
			DeviceSet.Main.Model.ApplyChanges();
		}

		public void ShowSettings() =>
			DeviceSet.Main.Messenger.Raise(new TransitionMessage(new KeyDeviceSetSettingsViewModel(DeviceSet.Main, DeviceSet.DeviceSet, Profile), nameof(KeyDeviceSetSettingsViewModel)));
	}
}
