using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MagicInput.Models
{
	public class NotifyObject : INotifyPropertyChanged
	{
		readonly Dictionary<string, object> values = new Dictionary<string, object>();

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected T GetValue<T>(T defaultValue = default(T), [CallerMemberName] string propertyName = null)
		{
			propertyName = propertyName.StartsWith("get_") ? propertyName.Replace("get_", null) : propertyName;

			if (!values.ContainsKey(propertyName))
				values[propertyName] = defaultValue;

			return (T)values[propertyName];
		}

		protected T SetValue<T>(T value, [CallerMemberName] string propertyName = null)
		{
			propertyName = propertyName.StartsWith("set_") ? propertyName.Replace("set_", null) : propertyName;
			values[propertyName] = value;

			OnPropertyChanged(propertyName);

			return value;
		}
	}
}
