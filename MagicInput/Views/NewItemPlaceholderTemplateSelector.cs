using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MagicInput.Views
{
	class NewItemPlaceholderTemplateSelector : DataTemplateSelector
	{
		public DataTemplate ItemTemplate { get; set; }
		public DataTemplate PlaceholderTemplate { get; set; }

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item == CollectionView.NewItemPlaceholder)
				return PlaceholderTemplate;
			else
				return ItemTemplate;
		}
	}
}
