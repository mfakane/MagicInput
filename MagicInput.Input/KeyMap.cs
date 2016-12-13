using System;
using System.Collections.Generic;
using System.ComponentModel;
using MagicInput.Input.Behaviors;
using MsgPack.Serialization;

namespace MagicInput.Input
{
	public class KeyMap : INotifyPropertyChanged
	{
		[MessagePackIgnore]
		public KeyProfile Profile { get; set; }
		public Guid Guid { get; private set; } = Guid.NewGuid();

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

		public IList<KeyBehavior> Behaviors { get; }

		public KeyMap() =>
			Behaviors = new TreeCollection<KeyBehavior, KeyMap>(this, i => i.KeyMap, (i, parent) => i.KeyMap = parent);

		public KeyMap(string name, IEnumerable<KeyInput> keys = null)
			: this()
		{
			Name = name;

			if (keys != null)
				foreach (var i in keys)
					Behaviors.Add(new FallbackBehavior(i));
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(PropertyChangedEventArgs e) =>
			PropertyChanged?.Invoke(this, e);
	}
}
