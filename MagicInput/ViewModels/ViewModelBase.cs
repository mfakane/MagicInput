using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Livet;

namespace MagicInput.ViewModels
{
	public class ViewModelBase : ViewModel
	{
		readonly Dictionary<string, object> values = new Dictionary<string, object>();

		protected T RegisterDisposable<T>(T obj)
			where T : IDisposable
		{
			CompositeDisposable.Add(obj);

			return obj;
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

			RaisePropertyChanged(propertyName);

			return value;
		}
	}
}
