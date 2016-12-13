using System;
using System.Collections.Generic;
using System.ComponentModel;
using MsgPack.Serialization;

namespace MagicInput.Input
{
	public class KeyDevice : INotifyPropertyChanged, IEquatable<KeyDevice>
	{
		[MessagePackIgnore]
		public KeyDeviceSet DeviceSet { get; set; }
		public Guid Guid { get; private set; } = Guid.NewGuid();
		public IList<KeyInput> Keys { get; }

		string _name;
		public string Name
		{
			get => _name ?? PhysicalDevice?.Name;
			set
			{
				if (value == "") value = null;

				if (_name != (_name = value))
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(Name)));
			}
		}

		KeyPhysicalDevice _physicalDevice;
		public KeyPhysicalDevice PhysicalDevice
		{
			get => _physicalDevice;
			set
			{
				if (PhysicalDevice != null)
					PhysicalDevice.Device = null;

				if (_physicalDevice != (_physicalDevice = value))
				{
					if (value != null)
						value.Device = this;

					OnPropertyChanged(new PropertyChangedEventArgs(nameof(PhysicalDevice)));
				}
			}
		}

		public KeyDevice() =>
			Keys = new TreeCollection<KeyInput, Guid>(() => Guid, i => i.DeviceGuid, (i, parent) => i.DeviceGuid = parent);

		public KeyDevice(KeyPhysicalDevice physicalDevice)
			: this() =>
			PhysicalDevice = physicalDevice;

		public KeyDevice(KeyPhysicalDevice physicalDevice, IEnumerable<KeyInput> keys)
			: this(physicalDevice)
		{
			foreach (var i in keys)
				Keys.Add(i);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(PropertyChangedEventArgs e) =>
			PropertyChanged?.Invoke(this, e);

		public IDisposable RegisterDevice() =>
			PhysicalDevice.RegisterDevice();

		public override bool Equals(object obj) =>
			this == obj || GetType() == obj?.GetType() && Equals((KeyDevice)obj);

		public bool Equals(KeyDevice other) =>
			other != null && (PhysicalDevice?.Equals(other.PhysicalDevice) ?? other.PhysicalDevice == null);

		public override int GetHashCode() =>
			typeof(KeyDevice).GetHashCode() ^ (PhysicalDevice?.GetHashCode() ?? 0);
	}
}
