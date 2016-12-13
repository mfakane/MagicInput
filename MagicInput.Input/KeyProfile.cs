using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using MsgPack.Serialization;

namespace MagicInput.Input
{
	public class KeyProfile : INotifyPropertyChanged
	{
		[MessagePackIgnore]
		public KeyDeviceSet DeviceSet { get; set; }
		[MessagePackIgnore]
		public Stack<Guid> PreviousKeyMaps { get; } = new Stack<Guid>();
		public IList<KeyMap> KeyMaps { get; }
		public Guid Guid { get; private set; } = Guid.NewGuid();

		KeyMap _currentKeyMap;
		[MessagePackIgnore]
		public KeyMap CurrentKeyMap
		{
			get => _currentKeyMap ?? KeyMaps.First();
			set
			{
				if (_currentKeyMap != (_currentKeyMap = value))
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(CurrentKeyMap)));
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

		string _targetProcessName;
		public string TargetProcessName
		{
			get => _targetProcessName;
			set
			{
				if (_targetProcessName != (_targetProcessName = value))
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(TargetProcessName)));
			}
		}

		TextMatchMethod _targetProcessNameMatchMethod;
		public TextMatchMethod TargetProcessNameMatchMethod
		{
			get => _targetProcessNameMatchMethod;
			set
			{
				if (_targetProcessNameMatchMethod != (_targetProcessNameMatchMethod = value))
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(TargetProcessNameMatchMethod)));
			}
		}

		string _targetWindowTitle;
		public string TargetWindowTitle
		{
			get => _targetWindowTitle;
			set
			{
				if (_targetWindowTitle != (_targetWindowTitle = value))
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(TargetWindowTitle)));
			}
		}

		TextMatchMethod _targetWindowTitleMatchMethod;
		public TextMatchMethod TargetWindowTitleMatchMethod
		{
			get => _targetWindowTitleMatchMethod;
			set
			{
				if (_targetWindowTitleMatchMethod != (_targetWindowTitleMatchMethod = value))
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(TargetWindowTitleMatchMethod)));
			}
		}

		public KeyProfile() =>
			KeyMaps = new TreeCollection<KeyMap, KeyProfile>(this, i => i.Profile, (i, parent) => i.Profile = parent);

		public KeyProfile(string name)
			: this() =>
			Name = name;

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(PropertyChangedEventArgs e) =>
			PropertyChanged?.Invoke(this, e);

		public void PushKeyMap(KeyMap keyMap)
		{
			PreviousKeyMaps.Push(CurrentKeyMap.Guid);
			CurrentKeyMap = keyMap;
		}

		public bool PopKeyMap()
		{
			if (!PreviousKeyMaps.Any()) return false;

			var guid = PreviousKeyMaps.Pop();

			if (!(KeyMaps.FirstOrDefault(i => i.Guid == guid) is KeyMap keyMap))
				return false;

			CurrentKeyMap = keyMap;

			return true;
		}

		public bool IsMatch(string processName, string windowTitle) =>
			TextMatches(TargetProcessNameMatchMethod, TargetProcessName, processName) &&
			TextMatches(TargetWindowTitleMatchMethod, TargetWindowTitle, windowTitle);

		static bool TextMatches(TextMatchMethod method, string text, string target)
		{
			if (method == TextMatchMethod.Ignore)
				return true;

			if (text == null || target == null)
				return false;

			switch (method)
			{
				case TextMatchMethod.Contains:
					return text.Contains(target);
				case TextMatchMethod.Exact:
					return text == target;
				case TextMatchMethod.Regex:
					return Regex.IsMatch(target, text);
				default:
					return false;
			}
		}
	}
}
