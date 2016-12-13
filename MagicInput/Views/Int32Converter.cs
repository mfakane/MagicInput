using System;
using System.Globalization;
using System.Windows.Data;

namespace MagicInput.Views
{
	class Int32Converter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
			System.Convert.ToInt32(value);

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			throw new NotImplementedException();
	}
}
