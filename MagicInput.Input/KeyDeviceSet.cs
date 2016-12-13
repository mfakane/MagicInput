using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MsgPack.Serialization;

namespace MagicInput.Input
{
	public class KeyDeviceSet : INotifyPropertyChanged
	{
		public IList<KeyDevice> Devices { get; }
		public IList<KeyProfile> Profiles { get; }

		string _name;
		public string Name
		{
			get => _name;
			set
			{
				if (_name != (_name = value))
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(Name)));
			}
		}

		KeyProfile _currentProfile;
		[MessagePackIgnore]
		public KeyProfile CurrentProfile
		{
			get => _currentProfile ?? Profiles.First();
			set
			{
				if (_currentProfile != (_currentProfile = value))
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(CurrentProfile)));
			}
		}

		public KeyDeviceSet()
		{
			Devices = new TreeCollection<KeyDevice, KeyDeviceSet>(this, i => i.DeviceSet, (i, parent) => i.DeviceSet = parent);
			Profiles = new TreeCollection<KeyProfile, KeyDeviceSet>(this, i => i.DeviceSet, (i, parent) => i.DeviceSet = parent);
		}

		public KeyDeviceSet(string name)
			: this() =>
			Name = name;

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(PropertyChangedEventArgs e) =>
			PropertyChanged?.Invoke(this, e);

		public KeyDevice GetDeviceFromGuid(Guid deviceGuid) =>
			Devices.FirstOrDefault(i => i.Guid == deviceGuid);

		public IEnumerable<KeyInput> GetAllKeys() =>
			Devices.SelectMany(i => i.Keys);

		public void ActiveWindowChanged(string processName, string windowTitle)
		{
			if (Profiles.LastOrDefault(i => i.IsMatch(processName, windowTitle)) is KeyProfile profile)
				CurrentProfile = profile;
		}
	}
}
