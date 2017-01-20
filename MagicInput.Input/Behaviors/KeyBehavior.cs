using System.ComponentModel;
using System.Linq;
using MsgPack.Serialization;

namespace MagicInput.Input.Behaviors
{
	[MessagePackRuntimeType]
	public abstract class KeyBehavior : INotifyPropertyChanged
	{
		[MessagePackIgnore]
		public KeyMap KeyMap { get; set; }
		public KeyInput Key { get; protected set; }
		public KeyDevice Device => KeyMap?.Profile?.DeviceSet.GetDeviceFromGuid(Key.DeviceGuid);

		bool _isDown;
		[MessagePackIgnore]
		public bool IsDown
		{
			get => _isDown;
			set
			{
				if (_isDown != (_isDown = value))
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsDown)));
			}
		}

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

		protected KeyBehavior()
		{
		}

		protected KeyBehavior(KeyInput key) =>
			Key = key;

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(PropertyChangedEventArgs e) =>
			PropertyChanged?.Invoke(this, e);

		public bool DoKeyDown(KeyData data)
		{
			if (IsDown)
				return OnKeyRepeat(data);

			IsDown = true;

			return OnKeyDown(data);
		}

		public void DoKeyUp(KeyData data)
		{
			if (!IsDown) return;

			IsDown = false;
			OnKeyUp(data);
		}

		protected abstract bool OnKeyDown(KeyData data);
		protected abstract bool OnKeyRepeat(KeyData data);
		protected abstract void OnKeyUp(KeyData data);

		public override string ToString() =>
			GetType().GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().First().DisplayName;
	}
}
