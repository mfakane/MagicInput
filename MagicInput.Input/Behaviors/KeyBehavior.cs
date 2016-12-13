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

		public bool DoKeyDown()
		{
			if (IsDown)
				return OnKeyRepeat();

			IsDown = true;

			return OnKeyDown();
		}

		public void DoKeyUp()
		{
			if (!IsDown) return;

			IsDown = false;
			OnKeyUp();
		}

		protected abstract bool OnKeyDown();
		protected abstract bool OnKeyRepeat();
		protected abstract void OnKeyUp();

		public override string ToString() =>
			GetType().GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().First().DisplayName;
	}
}
