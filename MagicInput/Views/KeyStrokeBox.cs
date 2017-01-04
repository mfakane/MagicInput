using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MagicInput.Input;

namespace MagicInput.Views
{
	public class KeyStrokeBox : TextBox
	{
		readonly HashSet<Key> currentModifiers = new HashSet<Key>();
		Key currentKey;

		public static readonly DependencyProperty VirtualKeysProperty = DependencyProperty.Register("VirtualKeys", typeof(VirtualKey[]), typeof(KeyStrokeBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public VirtualKey[] VirtualKeys
		{
			get => (VirtualKey[])GetValue(VirtualKeysProperty);
			set => SetValue(VirtualKeysProperty, value);
		}

		static KeyStrokeBox() =>
			DefaultStyleKeyProperty.OverrideMetadata(typeof(KeyStrokeBox), new FrameworkPropertyMetadata(typeof(KeyStrokeBox)));

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			if (!e.IsRepeat)
			{
				var key = e.Key == Key.System ? e.SystemKey : e.Key;

				if (IsModifierKey(key))
					currentModifiers.Add(key);
				else
					currentKey = e.Key;

				UpdateKeyProperties();
			}

			e.Handled = true;
			base.OnKeyDown(e);
		}

		protected override void OnPreviewKeyUp(KeyEventArgs e)
		{
			if (!e.IsRepeat)
			{
				var key = e.Key == Key.System ? e.SystemKey : e.Key;

				if (IsModifierKey(key))
					currentModifiers.Remove(key);
				else
					currentKey = Key.None;
			}

			e.Handled = true;
			base.OnPreviewKeyUp(e);
		}

		void UpdateKeyProperties()
		{
			VirtualKeys = currentModifiers.Concat(currentKey == Key.None ? Enumerable.Empty<Key>() : new[] { currentKey })
										  .Select(i => (VirtualKey)KeyInterop.VirtualKeyFromKey(i))
										  .ToArray();
		}

		bool IsModifierKey(Key key) =>
			key == Key.LeftCtrl || key == Key.RightCtrl ||
			key == Key.LeftShift || key == Key.RightShift ||
			key == Key.LeftAlt || key == Key.RightAlt ||
			key == Key.LWin || key == Key.RWin;
	}
}
