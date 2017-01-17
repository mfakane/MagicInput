using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Livet;
using MagicInput.Input;
using MagicInput.Input.Behaviors;

namespace MagicInput.ViewModels
{
	public class KeyDeviceSetSettingsViewModel : ViewModelBase
	{
		public MainViewModel Main { get; }
		public KeyDeviceSet DeviceSet { get; }
		public KeyPhysicalDevice[] AvailablePhysicalDevices { get; }
		public ObservableCollection<KeyDeviceSettingsViewModel> Devices { get; }
		public ObservableCollection<KeyProfileSettingsViewModel> Profiles { get; }
		public KeyProfileSettingsViewModel InitiallySelectedProfile { get; }
		public bool IsKeyAddMode { get; set; }
		public bool IsWindowActive { get; set; } = true;

		public KeyDeviceSetSettingsViewModel(MainViewModel main, KeyDeviceSet deviceSet)
		{
			Main = main;
			DeviceSet = deviceSet;
			AvailablePhysicalDevices = main.Model.Providers.SelectMany(i => i.GetAvailablePhysicalDevices()).ToArray();
			Devices = new ObservableCollection<KeyDeviceSettingsViewModel>(deviceSet.Devices.Select(i => new KeyDeviceSettingsViewModel(this, i, true))
				.Concat(AvailablePhysicalDevices.Select(i => new KeyDeviceSettingsViewModel(this, i.CreateDevice(), false))).Distinct(i => i.Device));
			Profiles = new ObservableCollection<KeyProfileSettingsViewModel>(deviceSet.Profiles.Select(i => new KeyProfileSettingsViewModel(i)));
		}

		public KeyDeviceSetSettingsViewModel(MainViewModel main, KeyDeviceSet deviceSet, KeyProfile initiallySelectedProfile)
			: this(main, deviceSet) =>
			CollectionViewSource.GetDefaultView(Profiles).MoveCurrentTo(InitiallySelectedProfile = Profiles.First(i => i.Profile == initiallySelectedProfile));

		public void Apply()
		{
			DeviceSet.Devices.Clear();

			foreach (var i in Devices.Where(i => i.PhysicalDevice != null && i.IsChecked))
			{
				i.Apply();
				DeviceSet.Devices.Add(i.Device);
			}

			DeviceSet.Profiles.Clear();

			foreach (var i in Profiles)
			{
				i.Apply();
				DeviceSet.Profiles.Add(i.Profile);
			}

			var keys = DeviceSet.GetAllKeys().Memoize();

			foreach (var i in DeviceSet.Profiles.SelectMany(i => i.KeyMaps))
			{
				var newSets = keys.Select(j => i.Behaviors.FirstOrDefault(bs => bs.Key == j) ?? new FallbackBehavior(j)).ToArray();

				i.Behaviors.Clear();

				foreach (var j in newSets)
					i.Behaviors.Add(j);
			}

			Main.Model.ApplyChanges();
		}

		public void AddDevice()
		{
			Devices.Add(new KeyDeviceSettingsViewModel(this, new KeyDevice(), false)
			{
				IsSelected = true,
			});
		}

		public void RemoveDevice(KeyDeviceSettingsViewModel device)
		{
			Devices.Remove(device);
			device.Dispose();
		}

		public void AddProfile()
		{
			Profiles.Add(new KeyProfileSettingsViewModel(new KeyProfile("プロファイル " + (Profiles.Count + 1))
			{
				KeyMaps =
				{
					new KeyMap("デフォルト", DeviceSet.GetAllKeys()),
				},
			}));
			CollectionViewSource.GetDefaultView(Profiles).MoveCurrentToLast();
		}

		public void RemoveProfile(KeyProfileSettingsViewModel profile)
		{
			Profiles.Remove(profile);
		}

		public void MoveProfileUp(KeyProfileSettingsViewModel profile)
		{
			var idx = Profiles.IndexOf(profile);

			Profiles.Move(idx, Math.Max(idx - 1, 0));
		}

		public void MoveProfileDown(KeyProfileSettingsViewModel profile)
		{
			var idx = Profiles.IndexOf(profile);

			Profiles.Move(idx, Math.Min(idx + 1, Profiles.Count - 1));
		}
	}

	public class KeyDeviceSettingsViewModel : ViewModelBase
	{
		bool isLoaded;
		IDisposable previewHandle;

		public KeyDeviceSetSettingsViewModel DeviceSet { get; }
		public KeyDevice Device { get; }
		public ObservableCollection<KeyInput> Keys { get; }

		public KeyPhysicalDevice PhysicalDevice
		{
			get => GetValue<KeyPhysicalDevice>();
			set
			{
				if (previewHandle != null)
				{
					CompositeDisposable.Remove(previewHandle);
					previewHandle.Dispose();
					previewHandle = null;
				}

				SetValue(value);
				RaisePropertyChanged(nameof(Name));

				if (isLoaded && value != null)
					CompositeDisposable.Add(previewHandle = CreatePreview());
			}
		}

		public bool IsSelected
		{
			get => GetValue<bool>();
			set => SetValue(value);
		}

		public bool IsChecked
		{
			get => GetValue<bool>();
			set => SetValue(value);
		}

		public KeyInput SelectedKey
		{
			get => GetValue<KeyInput>();
			set => SetValue(value);
		}

		public string Name
		{
			get => GetValue<string>() ?? PhysicalDevice?.Name;
			set => SetValue(string.IsNullOrEmpty(value) ? null : value);
		}

		public KeyDeviceSettingsViewModel(KeyDeviceSetSettingsViewModel deviceSet, KeyDevice device, bool isChecked)
		{
			DeviceSet = deviceSet;
			Device = device;
			Name = device.Name;
			PhysicalDevice = device.PhysicalDevice;
			Keys = new ObservableCollection<KeyInput>(device.Keys);
			IsChecked = isChecked;
		}

		public void Load(DependencyObject obj)
		{
			if (DesignerProperties.GetIsInDesignMode(obj))
				return;

			if (PhysicalDevice != null)
				CompositeDisposable.Add(previewHandle = CreatePreview());

			isLoaded = true;
		}

		protected override void Dispose(bool disposing)
		{
			isLoaded = false;
			previewHandle = null;
			base.Dispose(disposing);
		}

		IDisposable CreatePreview() =>
			PhysicalDevice.RegisterPreview(e => DispatcherHelper.UIDispatcher.InvokeAsync(() =>
			{
				if (!DeviceSet.IsWindowActive || !DeviceSet.IsKeyAddMode) return;

				if (e.PhysicalDevice.Kind != KeyDeviceKind.Mouse)
					IsChecked = IsSelected = true;

				if (Keys.Contains(e.Key))
					SelectedKey = e.Key;
				else
				{
					e.Key.DeviceGuid = Device.Guid;
					Keys.Add(e.Key);
				}
			}));

		public void Apply()
		{
			Device.Name = Name;
			Device.PhysicalDevice = PhysicalDevice;
			Device.Keys.Clear();

			foreach (var i in Keys)
				Device.Keys.Add(i);
		}

		public void RemoveKey(KeyInput key) =>
			Keys.Remove(key);

		public void MoveKeyUp(KeyInput key)
		{
			var idx = Keys.IndexOf(key);

			Keys.Move(idx, Math.Max(idx - 1, 0));
		}

		public void MoveKeyDown(KeyInput key)
		{
			var idx = Keys.IndexOf(key);

			Keys.Move(idx, Math.Min(idx + 1, Keys.Count - 1));
		}
	}

	public class KeyProfileSettingsViewModel : ViewModelBase
	{
		public KeyProfile Profile { get; }

		public string Name
		{
			get => GetValue<string>() ?? Profile.Name;
			set => SetValue(value);
		}

		public string TargetProcessName
		{
			get => GetValue<string>() ?? Profile.TargetProcessName;
			set => SetValue(value);
		}

		public TextMatchMethod TargetProcessNameMatchMethod
		{
			get => GetValue<TextMatchMethod?>() ?? Profile.TargetProcessNameMatchMethod;
			set => SetValue(value);
		}

		public string TargetWindowTitle
		{
			get => GetValue<string>() ?? Profile.TargetWindowTitle;
			set => SetValue(value);
		}

		public TextMatchMethod TargetWindowTitleMatchMethod
		{
			get => GetValue<TextMatchMethod?>() ?? Profile.TargetWindowTitleMatchMethod;
			set => SetValue(value);
		}

		public KeyProfileSettingsViewModel(KeyProfile profile) =>
			Profile = profile;

		public void Apply()
		{
			Profile.Name = Name;
			Profile.TargetProcessName = TargetProcessName;
			Profile.TargetProcessNameMatchMethod = TargetProcessNameMatchMethod;
			Profile.TargetWindowTitle = TargetWindowTitle;
			Profile.TargetWindowTitleMatchMethod = TargetWindowTitleMatchMethod;
		}
	}
}
