using System;
using MsgPack.Serialization;

namespace MagicInput.Input
{
	[MessagePackRuntimeType]
	public abstract class KeyPhysicalDevice : IEquatable<KeyPhysicalDevice>
	{
		[MessagePackIgnore]
		public KeyDevice Device { get; set; }
		public abstract string Name { get; }
		public abstract string ManufacturerName { get; }
		public abstract string Description { get; }
		public abstract KeyDeviceKind Kind { get; }

		public virtual KeyDevice CreateDevice() =>
			new KeyDevice(this);

		public abstract IDisposable RegisterDevice();
		public abstract IDisposable RegisterPreview(Action<KeyInputEventArgs> previewKeyInput);

		public override bool Equals(object obj) =>
			this == obj || GetType() == obj?.GetType() && Equals((KeyPhysicalDevice)obj);

		public abstract bool Equals(KeyPhysicalDevice other);
		public abstract override int GetHashCode();
	}
}
