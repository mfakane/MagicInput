using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MagicInput.Views
{
	static class TextBoxOptions
	{
		public static readonly DependencyProperty IsNumericProperty = DependencyProperty.RegisterAttached("IsNumeric", typeof(bool), typeof(TextBoxOptions), new PropertyMetadata((d, e) =>
		{
			var textBox = (TextBox)d;

			textBox.PreviewTextInput -= TextBox_PreviewTextInput;
			textBox.TextChanged -= TextBox_TextChanged;

			if ((bool)e.NewValue)
			{
				textBox.PreviewTextInput += TextBox_PreviewTextInput;
				textBox.TextChanged += TextBox_TextChanged;
			}
		}));

		static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) =>
			e.Handled = !e.Text.All(char.IsDigit);

		static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			var textBox = (TextBox)sender;

			textBox.Text = string.Join(null, textBox.Text?.Where(char.IsDigit).DefaultIfEmpty('0') ?? EnumerableEx.Return('0'));
		}

		public static bool GetIsNumeric(TextBox obj) => (bool)obj.GetValue(IsNumericProperty);
		public static void SetIsNumeric(TextBox obj, bool value) => obj.SetValue(IsNumericProperty, value);
	}
}
