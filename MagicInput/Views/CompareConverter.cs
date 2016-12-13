using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace MagicInput.Views
{
	class CompareConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Func<object, object, bool> predicate;
			object operand;

			if (parameter is string pstr)
			{
				string operandStr;

				switch (pstr[0])
				{
					case '=':
						operandStr = pstr.Substring(pstr[1] == '=' ? 2 : 1);
						predicate = (x, y) => Comparer.Default.Compare(x, y) == 0;

						break;
					case '!':
						operandStr = pstr.Substring(pstr.ElementAtOrDefault(1) == '=' ? 2 : 1);
						predicate = (x, y) => Comparer.Default.Compare(x, y) != 0;

						break;
					case '<':
						if (pstr[1] == '=')
						{
							operandStr = pstr.Substring(2);
							predicate = (x, y) => Comparer.Default.Compare(x, y) <= 0;
						}
						else
						{
							operandStr = pstr.Substring(1);
							predicate = (x, y) => Comparer.Default.Compare(x, y) < 0;
						}

						break;
					case '>':
						if (pstr[1] == '=')
						{
							operandStr = pstr.Substring(2);
							predicate = (x, y) => Comparer.Default.Compare(x, y) >= 0;
						}
						else
						{
							operandStr = pstr.Substring(1);
							predicate = (x, y) => Comparer.Default.Compare(x, y) > 0;
						}

						break;
					default:
						operandStr = pstr;
						predicate = (x, y) => Comparer.Default.Compare(x, y) == 0;

						break;
				}

				operand = string.IsNullOrEmpty(operandStr) || operandStr == "null" ? default(object) : int.Parse(operandStr);
			}
			else
			{
				predicate = (x, y) => x != null;
				operand = null;
			}

			return predicate(value, operand);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			throw new NotImplementedException();
	}
}
