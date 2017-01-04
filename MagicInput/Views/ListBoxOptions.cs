using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace MagicInput.Views
{
	static class ListBoxOptions
	{
		public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.RegisterAttached("SelectedItems", typeof(IList), typeof(ListBoxOptions), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) =>
		{
			var sender = (ListBox)d;
			var value = (IList)e.NewValue;

			sender.SelectionChanged -= Sender_SelectionChanged;
			sender.SelectedItems.Clear();

			if (value != null)
			{
				foreach (var i in value)
					sender.SelectedItems.Add(i);

				sender.SelectionChanged += Sender_SelectionChanged;
			}
		}));

		static void Sender_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var d = (ListBox)sender;
			var items = ((ListBox)sender).SelectedItems;
			var list = GetSelectedItems(d);

			if (list is Array)
			{
				var arr = Array.CreateInstance(list.GetType().GetElementType(), items.Count);

				for (var i = 0; i < arr.Length; i++)
					arr.SetValue(items[i], i);

				SetSelectedItems(d, arr);
			}
			else
			{
				foreach (var i in e.RemovedItems)
					items.Remove(i);

				foreach (var i in e.AddedItems)
					items.Add(i);
			}
		}

		public static IList GetSelectedItems(ListBox obj) => (IList)obj.GetValue(SelectedItemsProperty);
		public static void SetSelectedItems(ListBox obj, IList value) => obj.SetValue(SelectedItemsProperty, value);
	}
}
